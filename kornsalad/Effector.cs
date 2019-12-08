using OpenCvSharp;
using OpenCvSharp.Util;
using OpenCvSharp.XImgProc;
using System;
using System.Linq;

namespace kornsalad
{
    public class Effector
    {
        Mat Image { get; set; }
        int ImageHeight { get; set; }
        int ImageWidth { get; set; }
        int ImageChannel { get; set; }
        Size ImageSize { get; set; }
        Size SceneSize { get; set; }
        double Framerate { get; set; }
        VideoWriter TempWriter { get; set; }
        string Filename { get; set; }

        /// <summary>
        /// Make a effect base with provided image
        /// </summary>
        /// <param name="inputImageName"></param>
        /// <param name="frameRate"></param>
        /// <param name="sceneSize"></param>
        public Effector(string inputImageName, double frameRate, int[] sceneSize=null)
        {
            Image = Cv2.ImRead(inputImageName);
            ImageHeight = Image.Height;
            ImageWidth = Image.Width;
            ImageChannel = Image.Channels();
            ImageSize = new Size(ImageWidth, ImageHeight);
            Framerate = frameRate;

            if (sceneSize != null)
                SceneSize = new Size(sceneSize[0], sceneSize[1]);
            else SceneSize = ImageSize;
        }

        /// <summary>
        /// Merge upper effector with base effector
        /// </summary>
        /// <param name="Effector">Upper layer effect</param>
        public Effector(Effector upperEffector, Effector baseEffector)
        {

        }

        public void Blending()
        {

        }

        public void Initialize(string tempFilename = null, string preFormat = "mp4v")
        {
            if (tempFilename != null)
            {
                Filename = tempFilename;
                TempWriter = new VideoWriter(tempFilename, preFormat, Framerate, ImageSize);
            }
            else
            {
                Filename = Renamer.CreateANewName(directory: ".", extension: "mp4");
                TempWriter = new VideoWriter(Filename, preFormat, Framerate, ImageSize);
            }
        }

        public void Close()
            => TempWriter.Release();

        private Mat _Translate(Mat image, int x, int y)
        {
            var array = new float[,] {
                        { 1, 0, x},
                        { 0, 1, y}
                    };

            Mat filter = new MatOfFloat(2, 3);
            filter.SetArray(0, 0, array);

            return image.WarpAffine(filter, ImageSize);
        }

        private Mat _Rotate(Mat image, int angle, float[] center=null, double scale=1.0)
        {
            Point2f _center;

            if (center == null)
                _center = new Point2f(image.Width / 2, image.Height / 2);
            else _center = new Point2f(center[0] / 2, center[1] / 2);

            return image.WarpAffine(
                Cv2.GetRotationMatrix2D(_center, angle, scale),
                image.Size()
            );
        }

        public void Earthquake(int earthPower, int earthSpeed, int earthTime)
        {
            Random rd = new Random();
            
            for (var i = 0; i < Framerate * earthTime; i++)
            {
                if ((i % earthSpeed == 0) || (i == 0))
                {
                    TempWriter.Write(_Translate(Image, 
                        rd.Next(-earthPower, earthPower),
                        rd.Next(-earthPower, earthPower)));
                }
            }
        }
        
        public void Shake(int shakeDegree, int shakeSpeed, int shakeCount)
        {
            for (var i = 0; i < shakeCount; i++)
            {
                for (var j = 0; j < 2; j++)
                {
                    for (var k = 0; k < shakeDegree; k += shakeSpeed)
                        TempWriter.Write(_Rotate(Image, k));

                    for (var k = 0; k < 2; k++)
                        TempWriter.Write(_Rotate(Image, shakeDegree));

                    for (var k = shakeDegree; k < -shakeDegree; k -= shakeSpeed)
                        TempWriter.Write(_Rotate(Image, k));

                    for (var k = shakeDegree; k < 0; k -= shakeSpeed)
                        TempWriter.Write(_Rotate(Image, k));
                }
            }
        }

        public void Rotate(int degree, bool way, int speed)
        {
            if (way)
            {
                degree = -degree;
                speed = -speed;
            }

            for (var i = 0; i < degree; i += speed)
                TempWriter.Write(_Rotate(Image, i));

            TempWriter.Write(_Rotate(Image, degree));
        }

        public void FullRotate(int speed, bool way, int count)
        {
            if (way)
                speed = -speed;

            for (var i = 0; i < count; i++)
                for (var j = 0; j < 360; j += speed)
                    TempWriter.Write(_Rotate(Image, j));
        }

        public void Transition(int xDes, int yDes, int speed, int xPos, int yPos)
        {
            speed = speed * 24;
            xPos = 0;
            yPos = 0;

            for (var i = 0; i < speed; i++)
            {
                TempWriter.Write(_Translate(Image, xPos, -yPos));
                xPos = xPos + xDes / speed;
                yPos = yPos + yDes / speed;
            }
        }

        private Mat SetValueByCondition(Mat s, string _operator, int condValue,
            int valueToBeSet, bool valueAffect)
        {
            for (var k = 0; k < s.Rows; k++)
            {
                for (var l = 0; l < s.Cols; l++)
                {
                    switch(_operator)
                    {
                        case "<":
                            if (s.Get<int>(k, l) < condValue)
                            {
                                if (!valueAffect)
                                    s.Set<int>(k, l, valueToBeSet);
                                else
                                    s.Set<int>(k, l, s.Get<int>(k, l) + valueToBeSet);
                            }
                            break;
                        case ">":
                            if (s.Get<int>(k, l) > condValue)
                            {
                                if (!valueAffect)
                                    s.Set<int>(k, l, valueToBeSet);
                                else
                                    s.Set<int>(k, l, s.Get<int>(k, l) + valueToBeSet);
                            }
                            break;

                        case "<=":
                            if (s.Get<int>(k, l) <= condValue)
                            {
                                if (!valueAffect)
                                    s.Set<int>(k, l, valueToBeSet);
                                else
                                    s.Set<int>(k, l, s.Get<int>(k, l) + valueToBeSet);
                            }
                            break;

                        case ">=":
                            if (s.Get<int>(k, l) >= condValue)
                            {
                                if (!valueAffect)
                                    s.Set<int>(k, l, valueToBeSet);
                                else
                                    s.Set<int>(k, l, s.Get<int>(k, l) + valueToBeSet);
                            }
                            break;

                        case "==":
                            if (s.Get<int>(k, l) == condValue)
                            {
                                if (!valueAffect)
                                    s.Set<int>(k, l, valueToBeSet);
                                else
                                    s.Set<int>(k, l, s.Get<int>(k, l) + valueToBeSet);
                            }
                            break;
                        default:
                            throw new Exception("Unknown operator");
                    }
                }
            }

            return s;
        }

        /*
        public void Flash(int count)
        {
            var originalHsv = Image.CvtColor(ColorConversionCodes.BGR2HSV);
            var splitedHsv = originalHsv.Split();
            var h = splitedHsv[0].Clone();
            var s = splitedHsv[1].Clone();
            var v = splitedHsv[2].Clone();

            int increase = 43;

            for (var i = 0; i < count; i++)
            {
                for (var j = 0; j < 6; j++)
                {
                    var s1 = SetValueByCondition(s, "<", increase, increase, false);
                    var s2 = SetValueByCondition(s1, ">=", increase, increase, true);
                    var v1 = SetValueByCondition(v, ">", 255 - increase, 255, false);
                    var v2 = SetValueByCondition(v1, "<=", 255 - increase, increase, true);

                    Mat dstMat = originalHsv.Clone(); // hard-copy needed
                    Cv2.Merge(new Mat[] { h, s, v }, dstMat);

                    TempWriter.Write(dstMat.CvtColor(ColorConversionCodes.HSV2BGR));
                }

                for (var j = 0; j < 6; j++)
                {
                    var s1 = SetValueByCondition(s, ">", originalHsv[1] - increase, )
                }
            }
        }*/
    }
}
