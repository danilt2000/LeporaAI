using HepaticaAI.Core.Interfaces.Movement;
using Newtonsoft.Json;
using System.Diagnostics;
using WebSocketSharp;
using WebSocket = WebSocketSharp.WebSocket;
namespace HepaticaAI.Movement.Services
{
    internal class VtubeStudioMovement : IMovement
    {
        private static WebSocket ws = null!;
        private const string PLUGIN_NAME = "FaceMover";
        private const string PLUGIN_AUTHOR = "YourName";
        private static bool isAuthenticated = false;
        private static Timer movementTimer = null!;//Todo delete unused variables 
        private static Timer targetTimer = null!;
        private static Timer rightLookStartTimer = null!;
        private static Timer rightLookStopTimer = null!;
        private static bool isForcedRight = false;
        private static double mouthValue = 0;
        private const double FREQUENCY = 2.0;
        private static double eyeOpen = 2.0;
        private static bool isBlinking = false;
        private static DateTime blinkStartTime;
        private const int BLINK_DURATION = 2000;
        private static Timer updateMouthEyeTimer;

        private static double currentX = 0;
        private static double currentY = 0;
        private static double currentZ = 0;
        private static double targetX = 0;
        private static double targetY = 0;
        private static double targetZ = 0;

        private const double SMOOTHING_FACTOR = 0.1;
        private const int UPDATE_INTERVAL = 50;
        private const double MIN_TARGET_CHANGE = 5.0;
        private const double RIGHT_ANGLE = 25.0;
        private static readonly Random random = new Random();

        public string CurrentModelId { get; set; } = null!;

        public void Initialize()
        {
            ws = new WebSocket("ws://127.0.0.1:8001");
            ws.OnMessage += OnMessageReceived!;
            ws.OnError += (sender, e) => Debug.WriteLine($"WebSocket Error: {e.Message}");
            ws.OnClose += (sender, e) => Debug.WriteLine("WebSocket Closed");

            ws.Connect();

            RequestAuthenticationToken();
        }

        private static void RequestAuthenticationToken()
        {
            var tokenRequest = new
            {
                apiName = "VTubeStudioPublicAPI",
                apiVersion = "1.0",
                messageType = "AuthenticationTokenRequest",
                data = new
                {
                    pluginName = PLUGIN_NAME,
                    pluginDeveloper = PLUGIN_AUTHOR
                }
            };

            ws.Send(JsonConvert.SerializeObject(tokenRequest));
            Console.WriteLine("Sent authentication token request");
        }

        private void OnMessageReceived(object sender, MessageEventArgs e)
        {
            dynamic response = JsonConvert.DeserializeObject(e.Data)!;

            switch (response.messageType.ToString())
            {
                case "AuthenticationTokenResponse":
                    HandleAuthTokenResponse(response);
                    break;

                case "AuthenticationResponse":
                    HandleAuthResponse(response);
                    break;

                case "CurrentModelResponse":
                    CurrentModelId = response.data.modelID;
                    break;
            }
        }

        private static void HandleAuthTokenResponse(dynamic response)
        {
            var authRequest = new
            {
                apiName = "VTubeStudioPublicAPI",
                apiVersion = "1.0",
                messageType = "AuthenticationRequest",
                data = new
                {
                    pluginName = PLUGIN_NAME,
                    pluginDeveloper = PLUGIN_AUTHOR,
                    authenticationToken = response.data.authenticationToken.ToString()
                }
            };

            ws.Send(JsonConvert.SerializeObject(authRequest));
            Console.WriteLine("Sent authentication request");
        }

        private void HandleAuthResponse(dynamic response)
        {
            if (response.data.authenticated == true)
            {
                isAuthenticated = true;
                Console.WriteLine("Authentication successful!");
                SendRequestGetCurrentModelId();
                SendRequestGetHotkeysInCurrentModel();
                StartSmoothMovement();//Todo uncommit if it will be needed
                StartTargetUpdateTimer();
                StartRightLookTimers();
            }
            else
            {
                Console.WriteLine($"Authentication failed: {response.data.reason}");
            }
        }

        private static void StartSmoothMovement()
        {
            movementTimer = new Timer(_ =>
            {
                if (!isAuthenticated) return;

                currentX = Lerp(currentX, targetX, SMOOTHING_FACTOR);
                //currentY = Lerp(currentY, targetY, SMOOTHING_FACTOR);//TODO THINK ABOUT APPLYING IT 
                currentZ = Lerp(currentZ, targetZ, SMOOTHING_FACTOR);

                SendMovementCommand();
            }, null, 0, UPDATE_INTERVAL);
        }

        private static void StartTargetUpdateTimer()
        {
            targetTimer = new Timer(_ =>
            {
                if (!isForcedRight)
                {
                    GenerateNewTarget();
                    Console.WriteLine($"New target: X={targetX:F1}°, Y={targetY:F1}°, Z={targetZ:F1}°");
                }
            }, null, 0, 3000 + random.Next(2000));
        }

        private static void StartRightLookTimers()
        {
            rightLookStartTimer = new Timer(_ =>
            {
                if (!isForcedRight)
                {
                    isForcedRight = true;
                    targetY = RIGHT_ANGLE;
                    Console.WriteLine("Forcing right look for 4 seconds");

                    rightLookStopTimer.Change(4000, Timeout.Infinite);
                }
            }, null, 0, 10000);

            rightLookStopTimer = new Timer(_ =>
            {
                isForcedRight = false;
                GenerateNewTarget();
                Console.WriteLine("Returning to normal movement");
            }, null, Timeout.Infinite, Timeout.Infinite);
        }

        private static void GenerateNewTarget()
        {
            do
            {
                targetX = random.NextDouble() * 30 - 15;
            } while (Math.Abs(targetX - currentX) < MIN_TARGET_CHANGE);

            do
            {
                targetY = random.NextDouble() * 30 - 15;
            } while (Math.Abs(targetY - currentY) < MIN_TARGET_CHANGE);

            do
            {
                targetZ = random.NextDouble() * 90 - 45;
            } while (Math.Abs(targetZ - currentZ) < MIN_TARGET_CHANGE);
        }

        private static void SendMovementCommand()
        {
            var parameters = new[]
            {
            new { id = "FaceAngleX", value = Math.Round(currentX, 2), weight = 1.0 },
            new { id = "FaceAngleY", value = Math.Round(currentY, 2), weight = 1.0 },
            new { id = "FaceAngleZ", value = Math.Round(currentZ, 2), weight = 0.8 }
        };

            var command = new
            {
                apiName = "VTubeStudioPublicAPI",
                apiVersion = "1.0",
                messageType = "InjectParameterDataRequest",
                data = new
                {
                    faceFound = true,
                    mode = "set",
                    parameterValues = parameters
                }
            };

            ws.Send(JsonConvert.SerializeObject(command));
            Console.WriteLine($"Current position: X={currentX:F2}, Y={currentY:F2}, Z={currentZ:F2}");
        }

        private static double Lerp(double a, double b, double t)
        {
            return a + (b - a) * Math.Clamp(t, 0.0, 1.0);
        }

        public void StartIdleAnimation()
        {
            //RequestAuthenticationToken();
        }

        public void SendRequestGetCurrentModelId()
        {
            var command = new
            {
                apiName = "VTubeStudioPublicAPI",
                apiVersion = "1.0",
                messageType = "CurrentModelRequest"
            };

            ws.Send(JsonConvert.SerializeObject(command));
        }

        public void SendRequestGetHotkeysInCurrentModel()//Todo remove this if it's not used 
        {
            var command = new
            {
                apiName = "VTubeStudioPublicAPI",
                apiVersion = "1.0",
                messageType = "HotkeysInCurrentModelRequest"
            };

            ws.Send(JsonConvert.SerializeObject(command));
        }

        public void StartWinkAnimation()
        {
            var command = new
            {
                apiName = "VTubeStudioPublicAPI",
                apiVersion = "1.0",
                messageType = "HotkeyTriggerRequest",
                data = new
                {
                    hotkeyID = "56dfecb5ecfb4ac89d30cf559c4d7418"
                }
            };

            ws.Send(JsonConvert.SerializeObject(command));
        }

        private void StartUpdateMouthAndEyeTimer()
        {
            updateMouthEyeTimer = new Timer(UpdateMouthAndEye!, null, 0, UPDATE_INTERVAL);
        }

        public void DisableUpdateMouthAndEyeTimer()
        {
            updateMouthEyeTimer.Change(Timeout.Infinite, Timeout.Infinite);

            eyeOpen = 2.0;

            mouthValue = 0;

            SendMouthAndEyeOpenRequest();
        }

        private void UpdateMouthAndEye(object state)
        {
            if (!isAuthenticated)
                return;

            double time = DateTime.Now.TimeOfDay.TotalSeconds;
            mouthValue = (Math.Sin(2 * Math.PI * FREQUENCY * time) + 1) / 2;

            if (!isBlinking)
            {
                if (random.NextDouble() < 0.01)
                {
                    isBlinking = true;
                    blinkStartTime = DateTime.Now;
                }
            }

            if (isBlinking)
            {
                double elapsed = (DateTime.Now - blinkStartTime).TotalMilliseconds;
                if (elapsed < BLINK_DURATION / 2.0)
                {
                    double t = elapsed / (BLINK_DURATION / 2.0);
                    eyeOpen = 2 * (1 - t);
                }
                else if (elapsed < BLINK_DURATION)
                {
                    double t = (elapsed - BLINK_DURATION / 2.0) / (BLINK_DURATION / 2.0);
                    eyeOpen = 2 * t;
                }
                else
                {
                    eyeOpen = 2;
                    isBlinking = false;
                }
            }
            else
            {
                eyeOpen = 2;
            }

            SendMouthAndEyeOpenRequest();
        }

        public void SendMouthAndEyeOpenRequest()
        {
            var parameters = new[]
            {
                new { id = "MouthOpen", value = Math.Round(mouthValue, 2), weight = 1.0 },
                new { id = "EyeOpenRight", value = Math.Round(eyeOpen, 2), weight = 1.0 },
                new { id = "EyeOpenLeft", value = Math.Round(eyeOpen, 2), weight = 1.0 }
            };

            var command = new
            {
                apiName = "VTubeStudioPublicAPI",
                apiVersion = "1.0",
                messageType = "InjectParameterDataRequest",
                data = new
                {
                    faceFound = true,
                    mode = "set",
                    parameterValues = parameters
                }
            };

            ws.Send(JsonConvert.SerializeObject(command));
            Console.WriteLine($"MouthOpen: {mouthValue:F2}, EyeOpen: {eyeOpen:F2}");
        }

        public void OpenMouth()
        {
            StartUpdateMouthAndEyeTimer();
        }

        public void CloseMouth()
        {
            DisableUpdateMouthAndEyeTimer();
        }
    }
}
