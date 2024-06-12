using Microsoft.Azure.Kinect.BodyTracking;
using Microsoft.Azure.Kinect.Sensor;
using System;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;
using OpenCvSharp;

using AzureTracker = Microsoft.Azure.Kinect.BodyTracking.Tracker;
using Cv2Tracker = OpenCvSharp.Tracker;

namespace KinectRecordingApp
{
    public partial class MainForm : Form
    {
        // Global variables for managing Kinect recording
        private bool isRecording = false;
        private string videoFileName, eulerCsvFileName, quatCsvFileName;
        private Thread recordingThread;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "File Name Settings for Kinect Recording";

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                AutoSize = true
            };

            var dancerNameLabel = new Label { Text = "Dancer's Name:", AutoSize = true };
            var dancerNameInput = new TextBox { Name = "DancerNameInput" };

            var danceNumberLabel = new Label { Text = "Dance Number:", AutoSize = true };
            var danceNumberInput = new NumericUpDown { Name = "DanceNumberInput", Minimum = 1, Maximum = 100 };

            var trialNumberLabel = new Label { Text = "Trial Number:", AutoSize = true };
            var trialNumberInput = new NumericUpDown { Name = "TrialNumberInput", Minimum = 1, Maximum = 100 };

            var startButton = new Button { Text = "Start Recording", Name = "StartButton" };
            var stopButton = new Button { Text = "Stop Recording", Name = "StopButton", Enabled = false };

            startButton.Click += StartButton_Click;
            stopButton.Click += StopButton_Click;

            layout.Controls.Add(dancerNameLabel, 0, 0);
            layout.Controls.Add(dancerNameInput, 1, 0);
            layout.Controls.Add(danceNumberLabel, 0, 1);
            layout.Controls.Add(danceNumberInput, 1, 1);
            layout.Controls.Add(trialNumberLabel, 0, 2);
            layout.Controls.Add(trialNumberInput, 1, 2);
            layout.Controls.Add(startButton, 0, 3);
            layout.Controls.Add(stopButton, 1, 3);

            this.Controls.Add(layout);
            this.AutoSize = true;
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            //var dancerNameInput = (TextBox)this.Controls.Find("DancerNameInput", true)[0];
            var dancerNameInput = (TextBox)this.Controls.Find("DancerNameInput", true)[0];
            var danceNumberInput = (NumericUpDown)this.Controls.Find("DanceNumberInput", true)[0];
            var trialNumberInput = (NumericUpDown)this.Controls.Find("TrialNumberInput", true)[0];
            var startButton = (Button)sender;
            var stopButton = (Button)this.Controls.Find("StopButton", true)[0];

            string baseName = $"{dancerNameInput.Text}_{danceNumberInput.Value}_{trialNumberInput.Value}";
            videoFileName = baseName + ".mp4";
            eulerCsvFileName = baseName + "_euler.csv";
            quatCsvFileName = baseName + "_quat.csv";

            startButton.Enabled = false;
            stopButton.Enabled = true;
            isRecording = true;

            recordingThread = new Thread(RunKinectRecording);
            recordingThread.Start();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            isRecording = false;
            var stopButton = (Button)sender;
            var startButton = (Button)this.Controls.Find("StartButton", true)[0];

            stopButton.Enabled = false;
            startButton.Enabled = true;
        }

        private void RunKinectRecording()
        {
            // Capture and write pos data
            using (StreamWriter eulerSw = new StreamWriter(eulerCsvFileName, true))
            using (StreamWriter quatSw = new StreamWriter(quatCsvFileName, true))
            {
                // Open both devices
                using (Device device1 = Device.Open(0))
                using (Device device2 = Device.Open(1))
                {
                    // Start cameras with synchronized settings
                    device1.StartCameras(new DeviceConfiguration()
                    {
                        ColorFormat = ImageFormat.ColorBGRA32,
                        ColorResolution = ColorResolution.R1080p,
                        DepthMode = DepthMode.NFOV_Unbinned,
                        SynchronizedImagesOnly = true,
                        WiredSyncMode = WiredSyncMode.Master,
                        CameraFPS = FPS.FPS30
                    });

                    device2.StartCameras(new DeviceConfiguration()
                    {
                        ColorFormat = ImageFormat.ColorBGRA32,
                        ColorResolution = ColorResolution.R1080p,
                        DepthMode = DepthMode.NFOV_Unbinned,
                        SynchronizedImagesOnly = true,
                        WiredSyncMode = WiredSyncMode.Subordinate,
                        CameraFPS = FPS.FPS30
                    });

                    // Camera calibration
                    var device1Calibration = device1.GetCalibration();
                    var device2Calibration = device2.GetCalibration();

                    using (AzureTracker tracker1 = AzureTracker.Create(device1Calibration, new TrackerConfiguration() { ProcessingMode = TrackerProcessingMode.Gpu, SensorOrientation = SensorOrientation.Default }))
                    using (AzureTracker tracker2 = AzureTracker.Create(device2Calibration, new TrackerConfiguration() { ProcessingMode = TrackerProcessingMode.Gpu, SensorOrientation = SensorOrientation.Default }))
                    {
                        eulerSw.WriteLine("BodyID,JointIndex,PosX,PosY,PosZ,Roll,Pitch,Yaw");
                        quatSw.WriteLine("BodyID,JointIndex,PosX,PosY,PosZ,OriW,OriX,OriY,OriZ");

                        Cv2.NamedWindow("Kinect RGB View with Skeleton", WindowFlags.Normal);

                        while (isRecording)
                        {
                            using (Capture sensorCapture1 = device1.GetCapture())
                            using (Capture sensorCapture2 = device2.GetCapture())
                            {
                                tracker1.EnqueueCapture(sensorCapture1);
                                tracker2.EnqueueCapture(sensorCapture2);
                            }

                            using (Frame frame1 = tracker1.PopResult(TimeSpan.Zero, throwOnTimeout: false))
                            using (Frame frame2 = tracker2.PopResult(TimeSpan.Zero, throwOnTimeout: false))
                            {
                                if (frame1 != null && frame1.NumberOfBodies > 0)
                                {
                                    var skeleton1 = frame1.GetBodySkeleton(0);
                                    ProcessSkeleton(skeleton1, eulerSw, quatSw);
                                    DrawSkeleton(device1.GetColorImage(), skeleton1);
                                }

                                if (frame2 != null && frame2.NumberOfBodies > 0)
                                {
                                    var skeleton2 = frame2.GetBodySkeleton(0);
                                    ProcessSkeleton(skeleton2, eulerSw, quatSw);
                                    DrawSkeleton(device2.GetColorImage(), skeleton2);
                                }
                            }
                            Cv2.WaitKey(1);
                        }
                        Cv2.DestroyAllWindows();
                    }
                }
            }
        }

        private void ProcessSkeleton(Skeleton skeleton, StreamWriter eulerSw, StreamWriter quatSw)
        {
            for (int i = 0; i < (int)JointId.Count; i++)
            {
                var joint = skeleton.GetJoint((JointId)i);
                // Convert quaternion to Euler angles
                var euler = QuaternionToEuler(joint.Quaternion);

                eulerSw.WriteLine($"{skeleton.Id},{i},{joint.Position.X},{joint.Position.Y},{joint.Position.Z},{euler.Item1},{euler.Item2},{euler.Item3}");
                quatSw.WriteLine($"{skeleton.Id},{i},{joint.Position.X},{joint.Position.Y},{joint.Position.Z},{joint.Quaternion.W},{joint.Quaternion.X},{joint.Quaternion.Y},{joint.Quaternion.Z}");

                Console.WriteLine($"Joint {i}: Position=({joint.Position.X}, {joint.Position.Y}, {joint.Position.Z}), " +
                    $"Rotation (Euler)=({euler.Item1}, {euler.Item2}, {euler.Item3})");
            }
        }

        // Function to draw skeleton on the image
        private void DrawSkeleton(Image colorImage, Skeleton skeleton)
        {
            using (Mat colorMat = new Mat(colorImage.HeightPixels, colorImage.WidthPixels, MatType.CV_8UC4, colorImage.GetBuffer()))
            {
                for (int i = 0; i < (int)JointId.Count; i++)
                {
                    var joint = skeleton.GetJoint((JointId)i);
                    var point = new Point(joint.Position.X, joint.Position.Y);
                    Cv2.Circle(colorMat, point, 5, Scalar.Red, -1);
                }

                Cv2.ImShow("Kinect RGB View with Skeleton", colorMat);
                Cv2.WaitKey(1);
            }
        }

        // Function to convert quaternion to Euler angles (x, y, z)
        static Tuple<float, float, float> QuaternionToEuler(Quaternion quaternion)
        {
            float x, y, z;

            // Roll (x-axis rotation)
            double sinr_cosp = 2 * (quaternion.W * quaternion.X + quaternion.Y * quaternion.Z);
            double cosr_cosp = 1 - 2 * (quaternion.X * quaternion.X + quaternion.Y * quaternion.Y);
            x = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // Pitch (y-axis rotation)
            double sinp = 2 * (quaternion.W * quaternion.Y - quaternion.Z * quaternion.X);
            if (Math.Abs(sinp) >= 1)
                y = (float)(Math.CopySign(Math.PI / 2, sinp)); // use 90 degrees if out of range
            else
                y = (float)Math.Asin(sinp);

            // Yaw (z-axis rotation)
            double siny_cosp = 2 * (quaternion.W * quaternion.Z + quaternion.X * quaternion.Y);
            double cosy_cosp = 1 - 2 * (quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z);
            z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return new Tuple<float, float, float>(x, y, z);
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
