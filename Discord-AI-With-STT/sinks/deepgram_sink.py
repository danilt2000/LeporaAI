import asyncio
from asyncio import Queue
from enum import Enum
import json
import time

# 3rd party
from discord.sinks.core import Filters, Sink, default_filters
from deepgram import (
    DeepgramClient,
    DeepgramClientOptions,
    LiveTranscriptionEvents,
    LiveOptions,
)
import websockets

messages_queue_global = asyncio.Queue() 

class Speaker():
    class SpeakerState(Enum):
        RUNNING = 1
        TRANSCRIBE = 2
        FINALIZE = 3
        STOP = 4

    def __init__(self, loop: asyncio.BaseEventLoop, out_queue: Queue, deepgram_API_key: str,
                 sentence_end=300_000,  # 5 минут (примерно)
                 utterance_end=600_000  # 10 минут (примерно)
                 ):
        """
        Инициализация спикера.
        :param loop: event loop
        :param out_queue: очередь, куда отправляем распознанный текст
        :param deepgram_API_key: ваш Deepgram API ключ
        :param sentence_end: через сколько мс относительной тишины вставлять «пакет тишины» (для VAD)
        :param utterance_end: через сколько мс без новых данных финализировать высказывание
        """
        self.loop = loop
        self.queue = out_queue
        self.deepgram_API_key = deepgram_API_key

        self.sentence_end = sentence_end
        self.utterance_end = utterance_end

        self.user = None
        self.data = []
        self.new_bytes = False
        self.isNeededRetry = False
        self.messages_queue = Queue()
        self.last_byte = 0

        # Пакет тишины (fake audio), чтобы Deepgram понимал, что речь прервалась
        self.silent_packet = b"\x00" * 320

        self.state = self.SpeakerState.RUNNING

    def add_user(self, user_id):
        self.user = user_id
        # Запускаем задачу по подключению к Deepgram
        self.loop.create_task(self.deep_stream())

    def add_data(self, data, current_time):
        self.data.append(data)
        self.new_bytes = True
        self.last_byte = current_time

    def add_silence(self):
        self.data.append(self.silent_packet)

    def reset_data(self):
        self.data = []
        self.new_bytes = False

    async def deep_stream(self):
        """
        Основной цикл работы с Deepgram. Создает websocket и обрабатывает
        события, приходящие от Deepgram (промежуточные/финальные результаты).
        """
        
        outer = self
        global is_finals
        global queue
        global user
        global isNeededRetry
        global messages_queue

        user = self.user
        queue = self.queue
        isNeededRetry = self.isNeededRetry
        messages_queue = self.messages_queue
        is_finals = []

        try:
            config: DeepgramClientOptions = DeepgramClientOptions(
                options={"keepalive": "true"},
            )
            deepgram: DeepgramClient = DeepgramClient(self.deepgram_API_key, config)
            dg_connection = deepgram.listen.asyncwebsocket.v("1")

            # --- Обработчики событий Deepgram ---
            async def on_open(self, open, **kwargs):
                print("Подключение к Deepgram установлено.")

            async def on_message(self, result, **kwargs):
                global is_finals
                global queue
                sentence = result.channel.alternatives[0].transcript
                if len(sentence) == 0:
                    return
                if result.is_final:
                    # Частичное финализированное высказывание
                    is_finals.append(sentence)
                    if result.speech_final:
                        # Полное окончание речи
                        utterance = " ".join(is_finals)
                        print(f"Окончательная речь: {utterance}")
                    else:
                        print(f"Финальный фрагмент: {sentence}")
                else:
                    await messages_queue_global.put({"user": 0, "result": sentence, "type": "Intermediate speech"})
                    print(f"Промежуточный результат: {sentence}")

            async def on_metadata(self, metadata, **kwargs):
                print(f"Метаданные: {metadata}")

            async def on_speech_started(self, speech_started, **kwargs):
                print("Начало речи.")

            async def on_utterance_end(self, utterance_end, **kwargs):
                global is_finals
                global queue
                global user
                global messages_queue

                print("Конец фразы (Utterance End).")

                if len(is_finals) > 0:
                    utterance = " ".join(is_finals)
                    print(f"Сформированная фраза: {utterance}")
                    await queue.put({"user": user, "result": utterance})
                    await messages_queue.put({"user": user, "result": utterance})
                    await messages_queue_global.put({"user": user, "result": utterance})
                    is_finals = []

            async def on_close(self, close, **kwargs):
                print(f"\n\n{close}\n\n")
                reason = getattr(close, "reason", "Неизвестная причина")
                code = getattr(close, "code", "Неизвестный код")
                print(f"🔴 Соединение с Deepgram закрыто. Код: {code}, Причина: {reason}")
                print("Соединение с Deepgram закрыто.")
                global isNeededRetry
                isNeededRetry = True
                outer.isNeededRetry = True
                # self.state = self.SpeakerState.STOP

            async def on_error(self, error, **kwargs):
                print(f"Ошибка от Deepgram: {error}")

            async def on_unhandled(self, unhandled, **kwargs):
                print(f"Необработанное сообщение: {unhandled}")

            # Регистрируем обработчики
            dg_connection.on(LiveTranscriptionEvents.Open, on_open)
            dg_connection.on(LiveTranscriptionEvents.Transcript, on_message)
            dg_connection.on(LiveTranscriptionEvents.Metadata, on_metadata)
            dg_connection.on(LiveTranscriptionEvents.SpeechStarted, on_speech_started)
            dg_connection.on(LiveTranscriptionEvents.UtteranceEnd, on_utterance_end)
            dg_connection.on(LiveTranscriptionEvents.Close, on_close)
            dg_connection.on(LiveTranscriptionEvents.Error, on_error)
            dg_connection.on(LiveTranscriptionEvents.Unhandled, on_unhandled)

            # Настраиваем параметры распознавания.
            # Для русского языка: language="ru"
            # Увеличиваем utterance_end_ms и endpointing, чтобы редко завершать речь
            options: LiveOptions = LiveOptions(
                model="nova-2-general",         # проверьте, что модель поддерживает русский
                language="ru",          # RU
                smart_format=True,
                encoding="linear16",
                channels=2,
                sample_rate=48000,
                interim_results=True,
                utterance_end_ms=f"{self.utterance_end}",  # (здесь 600000 мс = 600с = 10 мин)
                vad_events=True,
                endpointing=self.sentence_end,            # (здесь 300000 мс = 5 мин)
            )

            addons = {
                "no_delay": "true"
            }

            # Подключаемся к Deepgram c нужными опциями
            if await dg_connection.start(options, addons=addons) is False:
                print("Не удалось подключиться к Deepgram.")
                return

            # async def keep_alive():
            #         while True:
            #             await asyncio.sleep(10)
            #             try:
            #                 await dg_connection.send(b"")
            #                 print("🔄 Отправлен keep-alive пакет (тишина).")
            #             except Exception as e:
            #                 print(f"⚠️ Ошибка keep-alive: {e}")

            # self.loop.create_task(keep_alive())

            # Основной цикл, пока объект не переведён в состояние STOP
            while self.state != self.SpeakerState.STOP:
                if self.state == self.SpeakerState.TRANSCRIBE:
                    # Отправляем накопленные данные
                    await dg_connection.send(b"".join(self.data))
                    self.reset_data()
                    self.state = self.SpeakerState.RUNNING

                elif self.state == self.SpeakerState.FINALIZE:
                    # Явная финализация, если мы хотим закрыть фразу
                    await dg_connection.finalize()
                    self.reset_data()
                    self.state = self.SpeakerState.RUNNING
                if isNeededRetry:
                    await dg_connection.finish()
                    await asyncio.sleep(1)
                    break
                else:
                    # немножко ждем, чтобы не нагружать цикл
                    await asyncio.sleep(.005)

            # Если состояние STOP — завершаем соединение корректно
            await dg_connection.finish()
            await asyncio.sleep(1)

        except Exception as e:
            print(f"Не удалось открыть сокет к Deepgram: {e}")
            return

class DeepgramSink(Sink):

    class SinkSettings:
        def __init__(self,
                     deepgram_API_key: str,
                     sentence_end=300_000,
                     utterence_end=600_000,
                     data_length=25000,
                     max_speakers=-1):
            """
            :param deepgram_API_key: Deepgram API ключ
            :param sentence_end: После какого времени бездействия вставлять "тишину" (мс)
            :param utterence_end: После какого времени бездействия финализировать фразу (мс)
            :param data_length: Максимальная длина буфера (байт)
            :param max_speakers: Максимальное число спикеров (-1 = без лимита)
            """
            self.deepgram_API_key = deepgram_API_key
            self.sentence_end = sentence_end
            self.utterence_end = utterence_end
            self.data_length = data_length
            self.max_speakers = max_speakers

    def __init__(self, *,
                 filters=None,
                 sink_settings: SinkSettings,
                 queue: asyncio.Queue,
                 loop: asyncio.AbstractEventLoop):
        if filters is None:
            filters = default_filters
        self.filters = filters
        Filters.__init__(self, **self.filters)
        self.lock = asyncio.Lock()
        self.sink_settings = sink_settings
        self.queue = queue
        self.loop = loop
        self.connected_clients = set()
        self.vc = None
        self.running = True

        self.voice_queue = Queue()
        
        self.speakers = []

        self.loop.create_task(self.start_local_server())

        self.loop.create_task(self.insert_voice())

        self.loop.create_task(self.start_processing())

    async def local_server(self, websocket):
        """Обработчик входящих WebSocket-соединений."""
        self.connected_clients.add(websocket)  
        try:
            async for message in websocket:
                data = json.loads(message)
                print(f"📥 Получено от {data['user']}: {data['result']}")
        finally:
            self.connected_clients.remove(websocket)
    async def process_queue(self):
        """
        Обрабатывает сообщения из messages_queue_global в фоновом режиме
        и отправляет их в WebSocket.
        """
        while True:
            try:
                message = await messages_queue_global.get()  # Получаем сообщение
                
                if message is not None:
                    message_json = json.dumps(message)
                    
                    for ws in self.connected_clients:
                        try:
                            await ws.send(message_json)
                            print(f"📤 Отправлено в WebSocket: {message_json}")
                        except Exception as ws_error:
                            print(f"⚠️ Ошибка отправки WebSocket: {ws_error}")

                    messages_queue_global.task_done()  # Завершаем обработку
                
            except Exception as e:
                print(f"⚠️ Ошибка при обработке сообщения в очереди: {e}")

    async def start_processing(self):
        """Запускает обработку очереди в фоновом режиме"""
        asyncio.create_task(self.process_queue())  # Запуск без блокировки основного потока

    async def start_local_server(self):
        """Запуск локального WebSocket-сервера в фоновом режиме"""
        server = await websockets.serve(self.local_server, "localhost", 8765)
        print("✅ Локальный WebSocket-сервер запущен на ws://localhost:8765")
        await server.wait_closed()

    async def insert_voice(self):
        """
        Цикл, который берет пакеты из self.voice_queue и распределяет их
        по нужному Speaker, в зависимости от user_id. Потом Speaker
        отправляет их в Deepgram для распознавания.
        """
        while self.running:
            current_time = time.time()
            if not self.voice_queue.empty():
                while not self.voice_queue.empty():
                    item = await self.voice_queue.get()
                    user_id = item[0]
                    data_bytes = item[1]

                    user_exists = False
                    for speaker in self.speakers:
                        if speaker.user is None:
                            speaker.add_user(user_id)

                        if speaker.isNeededRetry:
                            await asyncio.sleep(1)
                            await self.remove_speaker(speaker.user)
                            continue
                        
                        if user_id == speaker.user:
                            speaker.add_data(data_bytes, current_time)
                            user_exists = True
                            break
                    
                    if not user_exists:
                        # Создаем нового спикера, если лимит не превышен
                        # if self.sink_settings.max_speakers < 0 or \
                        #    len(self.speakers) < self.sink_settings.max_speakers:
                        sp = Speaker(
                                self.loop,
                                self.queue,
                                self.sink_settings.deepgram_API_key,
                                self.sink_settings.sentence_end,
                                self.sink_settings.utterence_end
                        )
                        self.speakers.append(sp)
                        sp.add_user(user_id)
                        sp.add_data(data_bytes, current_time)
            else:
                await asyncio.sleep(.02)

            # Проверяем, нужно ли отправлять данные на финализацию
            for speaker in self.speakers:
                #Transcribe when new data is available
                if speaker.new_bytes:
                    speaker.state = speaker.SpeakerState.TRANSCRIBE
                #finalize data if X seconds passes from last data packet from discord
                elif current_time > speaker.last_byte + speaker.utterance_end/1000:
                    speaker.state = speaker.SpeakerState.FINALIZE   
                #add silence to help process utterance
                elif  current_time > speaker.last_byte + speaker.sentence_end/1000:
                    speaker.add_silence()
        # Итерируем по копии списка, чтобы можно было безопасно удалять элементы
            # for speaker in self.speakers[:]:# THIS FUNCTIONALITY OF REMOVING THE SAME USER IS UNSTABLE TEST IT ONCE BUG WITH SAME USE APPEAR !!!!!!!!!
            #     if speaker.user in seen_users:
            #         # Если пользователь с таким id уже встречался, удаляем спикера
            #         await self.remove_speaker(speaker.user)
            #         self.speakers.remove(speaker)
            #     else:
            #         seen_users.add(speaker.user)
                # Если прошло много времени с последнего байта => FINALIZE
                # elif current_time > speaker.last_byte + (speaker.utterance_end / 1000):
                #     # Можно убрать этот блок, если хотите вообще не финализировать
                #     speaker.state = speaker.SpeakerState.FINALIZE

                # Если прошло sentence_end мс с последнего байта => добавляем тишину
                # elif current_time > speaker.last_byte + (speaker.sentence_end / 1000):
                #     speaker.add_silence()

        # Если running=False, останавливаем все Speaker
        for speaker in self.speakers:
            speaker.state = speaker.SpeakerState.STOP
     
    async def remove_speaker(self, user_id):
        try:
            for speaker in self.speakers.copy():
                if speaker.user == user_id and speaker.isNeededRetry:
                    self.speakers.remove(speaker)
                    print(f"Speaker с user_id {user_id} удалён из списка speakers.")
        except Exception as e:
            print(f"Ошибка в remove_speaker: {e}")
        

    @Filters.container
    def write(self, data, user_id):
        """
        Вызывается при получении очередного пакета аудио от Discord.
        :param data: аудиоданные
        :param user_id: ID пользователя, который говорит
        """
        try:
            data_len = len(data)
            if data_len > self.sink_settings.data_length:
                cutoff = self.sink_settings.data_length - int(self.sink_settings.data_length / 10)
                data = data[-cutoff:]

            self.voice_queue.put_nowait([user_id, data])

        except Exception as e:
        # Можно записать в логи или вывести сообщение
            print(f"Ошибка в write(): {e}")

    def close(self):
        """
        Останавливает основной цикл вставки голоса и останавливает всех спикеров.
        """
        self.running = False
        self.queue.put_nowait(None)  # Положим None, чтобы в другом месте завершался цикл
