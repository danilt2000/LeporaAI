using HepaticaAI.Brain.Models;
using HepaticaAI.Core.Interfaces.Memory;
using Newtonsoft.Json;

namespace HepaticaAI.Brain
{
    class SystemPromptsUpdater : ISystemPromptsUpdater
    {
        private readonly string _projectRoot;
        private readonly string _binFolder;
        private readonly List<string> _jsonFileNames = new()
        {
            "character_personality.json",
            "character_personality_temp_first.json",
            "character_personality_temp_second.json"
        };

        public SystemPromptsUpdater()
        {
            _projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName;
            _binFolder = AppDomain.CurrentDomain.BaseDirectory; 
        }

        public void UpdateSystemPrompt(string systemPromptsJsonName, string systemPrompts)
        {
            try
            {
                string rootPath = Path.Combine(_projectRoot, systemPromptsJsonName);
                string binPath = Path.Combine(_binFolder, systemPromptsJsonName);

                if (File.Exists(rootPath))
                {
                    File.WriteAllText(rootPath, systemPrompts);
                    Console.WriteLine($"✅ JSON '{systemPromptsJsonName}' обновлён в проекте!");
                }
                else
                {
                    Console.WriteLine($"❌ Файл '{systemPromptsJsonName}' не найден в корне проекта!");
                }

                if (File.Exists(binPath))
                {
                    File.WriteAllText(binPath, systemPrompts);
                    Console.WriteLine($"✅ JSON '{systemPromptsJsonName}' обновлён в bin!");
                }
                else
                {
                    Console.WriteLine($"⚠ Файл '{systemPromptsJsonName}' отсутствует в bin. Создаём...");
                    File.WriteAllText(binPath, systemPrompts);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Ошибка при обновлении JSON: {ex.Message}");
            }
        }

        public string GetCharacterName()
        {
            string filePath = Path.Combine(_projectRoot, "character_info.json");

            if (File.Exists(filePath))
            {
                try
                {
                    string jsonContent = File.ReadAllText(filePath);
                    var character = JsonConvert.DeserializeObject<CharacterJson>(jsonContent);
                    return character?.CharacterName!;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠ Ошибка при чтении или десериализации файла '{filePath}': {ex.Message}");
                    return "Unknown";
                }
            }
            else
            {
                Console.WriteLine($"❌ Файл 'character_info.json' не найден!");
                return "Unknown";
            }
        }

        public void SetCharacterName(string newCharacterName)
        {
            string filePath = Path.Combine(_projectRoot, "character_info.json");

            if (File.Exists(filePath))
            {
                try
                {
                    string jsonContent = File.ReadAllText(filePath);
                    var character = JsonConvert.DeserializeObject<CharacterJson>(jsonContent);

                    if (character != null)
                    {
                        character.CharacterName = newCharacterName;
                        string updatedJson = JsonConvert.SerializeObject(character, Formatting.Indented);
                        File.WriteAllText(filePath, updatedJson);
                        Console.WriteLine($"✅ Имя персонажа обновлено на '{newCharacterName}' в файле '{filePath}'");
                    }
                    else
                    {
                        Console.WriteLine("❌ Ошибка при десериализации JSON!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠ Ошибка при обновлении файла '{filePath}': {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"❌ Файл 'character_info.json' не найден для обновления имени!");
            }
        }

        public List<string> GetAllSystemPrompts()
        {
            var jsonList = new List<string>();

            foreach (string fileName in _jsonFileNames)
            {
                try
                {
                    string filePath = Path.Combine(_projectRoot, fileName);

                    if (File.Exists(filePath))
                    {
                        string jsonContent = File.ReadAllText(filePath);
                        jsonList.Add(jsonContent);
                    }
                    else
                    {
                        Console.WriteLine($"❌ Файл '{fileName}' не найден!");
                        jsonList.Add(string.Empty);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠ Ошибка при чтении файла '{fileName}': {ex.Message}");
                    jsonList.Add(string.Empty);
                }
            }

            return jsonList;
        }
    }
}
