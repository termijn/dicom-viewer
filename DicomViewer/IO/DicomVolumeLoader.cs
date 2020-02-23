using Dicom;
using Dicom.Imaging;
using Dicom.Imaging.Codec;
using Dicom.Imaging.Render;
using Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DicomViewer.IO
{
    public static class DicomVolumeLoader
    {
        private static ILogger logger;

        static DicomVolumeLoader()
        {
            logger = Logging.GetLogger("DicomVolumeLoader");
        }   

        public static Scan Load(DicomSeries series)
        {
            try
            {
                if (series.FileNames.Count == 1)
                {
                    var filePath = series.FileNames.First();
                    var file = DicomFile.Open(filePath);
                    return Load(file);
                }
                else if (series.FileNames.Count > 1)
                {
                    var files = new List<DicomFile>();
                    foreach (var fileName in series.FileNames)
                    {
                        var file = DicomFile.Open(fileName);
                        files.Add(file);
                    }
                    return Load(files);
                }
            }
            catch (Exception e)
            {
                logger.LogWarning(e, $"Could not load dataset '{e.Message}' {e.StackTrace}");
            }
            return null;
        }

        private static Scan LoadXA3DImageStorage(DicomFile dicomFile)
        {
            logger.LogInformation("Loading XA 3D Image file");

            var volume = new ImageSet();
            var dataSet = dicomFile.Dataset;

            var dicomPixelData = DicomPixelData.Create(dataSet);
            var nrFrames = dicomPixelData.NumberOfFrames;

            var functionalGroupShared = dataSet.GetSequence(DicomTag.SharedFunctionalGroupsSequence);
            var frameDataSet = functionalGroupShared.ElementAt(0);

            for (int i = 0; i < nrFrames; i++)
            {
                var pixelData = PixelDataFactory.Create(dicomPixelData, i);

                ImageData image = CreateImageData(pixelData);
                if (image == null) { continue; }
                volume.Slices.Add(image);

                var pixelMeasures = frameDataSet.GetSequence(DicomTag.PixelMeasuresSequence);
                var pixelMeasuresDataSet = pixelMeasures.ElementAt(0);
                GetPixelSpacing(pixelMeasuresDataSet, image);                

                var planeOrientations = frameDataSet.GetSequence(DicomTag.PlaneOrientationSequence);
                var planeOrientationDataSet = planeOrientations.ElementAt(0);
                var dicomOrientationPatient = planeOrientationDataSet.GetDicomItem<DicomDecimalString>(DicomTag.ImageOrientationPatient);
                GetImageOrientationPatient(image, dicomOrientationPatient);

                var functionalGroupPerFrame = dataSet.GetSequence(DicomTag.PerFrameFunctionalGroupsSequence);
                var functionalGroupPerFrameDataSet = functionalGroupPerFrame.ElementAt(i);

                var planePositionSequence = functionalGroupPerFrameDataSet.GetSequence(DicomTag.PlanePositionSequence);
                var planePositionDataSet = planePositionSequence.ElementAt(0);
                GetPositionPatient(planePositionDataSet, image);
                
                ReadWindowing(image, frameDataSet);
                ReadPixelTransformation(image, frameDataSet, functionalGroupPerFrameDataSet);
            }

            DeriveVolumeFields(volume);
            var patient = GetPatient(dataSet);

            return new Scan { Volume = volume, Patient = patient };
        }

        private static void DeriveVolumeFields(ImageSet volume)
        {
            var firstImage = volume.Slices.First();
            var zAxis = firstImage.XAxisPatient.Cross(firstImage.YAxisPatient);
            
            volume.TransformationToPatient = ToRotationMatrix(firstImage.XAxisPatient, firstImage.YAxisPatient);
            volume.RescaleIntercept = firstImage.Intercept;
            volume.RescaleSlope = firstImage.Slope;
            volume.Dimensions = new Dimensions3
            {
                X = volume.Slices[0].Width,
                Y = volume.Slices.Count,
                Z = volume.Slices[0].Height
            };

            volume.Slices.Sort(new SlicePositionComparer(zAxis));
            volume.VoxelSpacing = GetVoxelSpacing(volume);
        }

        private static void GetImageOrientationPatient(ImageData image, DicomDecimalString dicomOrientationPatient)
        {
            if (dicomOrientationPatient != null)
            {
                var xAxis = new Vector3
                {
                    X = dicomOrientationPatient.Get<double>(0),
                    Y = dicomOrientationPatient.Get<double>(1),
                    Z = dicomOrientationPatient.Get<double>(2)
                };
                var yAxis = new Vector3
                {
                    X = dicomOrientationPatient.Get<double>(3),
                    Y = dicomOrientationPatient.Get<double>(4),
                    Z = dicomOrientationPatient.Get<double>(5)
                };
                image.XAxisPatient = xAxis.Normalized();
                image.YAxisPatient = yAxis.Normalized();
            }
        }

        private static void GetPositionPatient(DicomDataset dataSet, ImageData image)
        {
            var dicomPositionPatient = dataSet.GetDicomItem<DicomDecimalString>(DicomTag.ImagePositionPatient);
            if (dicomPositionPatient != null)
            {
                image.PositionPatient.X = dicomPositionPatient.Get<double>(0);
                image.PositionPatient.Y = dicomPositionPatient.Get<double>(1);
                image.PositionPatient.Z = dicomPositionPatient.Get<double>(2);
            }
        }

        private static void GetPixelSpacing(DicomDataset dataSet, ImageData image)
        {
            var dicomPixelSpacing = dataSet.GetDicomItem<DicomDecimalString>(DicomTag.PixelSpacing);
            if (dicomPixelSpacing != null)
            {
                image.PixelSpacing.X = dicomPixelSpacing.Get<double>(0);
                image.PixelSpacing.Y = dicomPixelSpacing.Get<double>(1);
            }
        }

        private static void ReadWindowing(ImageData image, DicomDataset functionalGroupPerFrameDataSet)
        {
            DicomSequence frameVOILUTSequence;
            if (functionalGroupPerFrameDataSet.TryGetSequence(DicomTag.FrameVOILUTSequence, out frameVOILUTSequence))
            {
                var frameVOILUT = frameVOILUTSequence.ElementAt(0);
                var windowLevel = frameVOILUT.GetSingleValueOrDefault<double>(DicomTag.WindowCenter, 1000);
                var windowWidth = frameVOILUT.GetSingleValueOrDefault<double>(DicomTag.WindowWidth, 2000);
                image.WindowLevel = windowLevel;
                image.WindowWidth = windowWidth;
                image.DefaultWindowingAvailable = true;
            }
        }

        private static void ReadPixelTransformation(ImageData image, DicomDataset frameDataSet, DicomDataset functionalGroupPerFrameDataSet)
        {
            DicomSequence pixelValueTransformationSequence;
            bool pixelValueTransformationSequenceFound =
                frameDataSet.TryGetSequence(DicomTag.PixelValueTransformationSequence, out pixelValueTransformationSequence) ||
                functionalGroupPerFrameDataSet.TryGetSequence(DicomTag.PixelValueTransformationSequence, out pixelValueTransformationSequence);

            if (pixelValueTransformationSequenceFound)
            {
                var pixelValueTransformation = pixelValueTransformationSequence.ElementAt(0);
                var frameIntercept = pixelValueTransformation.GetSingleValueOrDefault<double>(DicomTag.RescaleIntercept, 0);
                var frameSlope = pixelValueTransformation.GetSingleValueOrDefault<double>(DicomTag.RescaleSlope, 1);
                image.Intercept = frameIntercept;
                image.Slope = frameSlope;
            }
        }

        private static Scan Load(DicomFile dicomFile)
        {
            var dataSet = dicomFile.Dataset;
            var sopclass = dataSet.GetValueOrDefault(DicomTag.SOPClassUID, 0, string.Empty);

            switch(sopclass)
            {
                case DicomSopClasses.XA3DImageStorageSopClass:
                    return LoadXA3DImageStorage(dicomFile);
                case DicomSopClasses.EnhancedMRImageStorageSopClass:
                    return LoadEnhancedMRImage(dicomFile);
                case DicomSopClasses.MRImageStorageSopClass:
                case DicomSopClasses.XRayAngiographicImageStorageSopClass:
                    return Load(new List<DicomFile> { dicomFile });
                default:
                    logger.LogWarning($"Unsupported sop class: {sopclass}");
                    break;
            };            
            return null;
        }

        private static Scan LoadEnhancedMRImage(DicomFile dicomFile)
        {
            var volume = new ImageSet();
            var dataSet = dicomFile.Dataset;

            var dicomPixelData = DicomPixelData.Create(dataSet);
            var nrFrames = dicomPixelData.NumberOfFrames;

            var functionalGroupShared = dataSet.GetSequence(DicomTag.SharedFunctionalGroupsSequence);
            var frameDataSet = functionalGroupShared.ElementAt(0);
            DicomDataset functionalGroupPerFrameDataSet = null;
            for (int i = 0; i < nrFrames; i++)
            {
                var pixelData = PixelDataFactory.Create(dicomPixelData, i);

                ImageData image = CreateImageData(pixelData);
                if (image == null) { continue; }

                volume.Slices.Add(image);

                var functionalGroupPerFrame = dataSet.GetSequence(DicomTag.PerFrameFunctionalGroupsSequence);
                functionalGroupPerFrameDataSet = functionalGroupPerFrame.ElementAt(i);
                var pixelMeasureSequence = functionalGroupPerFrameDataSet.GetSequence(DicomTag.PixelMeasuresSequence);
                var pixelMeasureSequenceItem = pixelMeasureSequence.FirstOrDefault();

                GetPixelSpacing(pixelMeasureSequenceItem, image);                

                var planeOrientations = functionalGroupPerFrameDataSet.GetSequence(DicomTag.PlaneOrientationSequence);
                var planeOrientationDataSet = planeOrientations.ElementAt(0);
                var dicomOrientationPatient = planeOrientationDataSet.GetDicomItem<DicomDecimalString>(DicomTag.ImageOrientationPatient);
                GetImageOrientationPatient(image, dicomOrientationPatient);

                var planePositionSequence = functionalGroupPerFrameDataSet.GetSequence(DicomTag.PlanePositionSequence);
                var planePositionDataSet = planePositionSequence.ElementAt(0);
                GetPositionPatient(planePositionDataSet, image);

                ReadWindowing(image, functionalGroupPerFrameDataSet);
                ReadPixelTransformation(image, frameDataSet, functionalGroupPerFrameDataSet);
            }

            DeriveVolumeFields(volume);
            var patient = GetPatient(dataSet);

            return new Scan { Volume = volume, Patient = patient };
        }

        private static Scan Load(List<DicomFile> files)
        {
            if (files.Count == 0) { return null; }

            var volume = new ImageSet();

            foreach (var originalFile in files)
            {
                // This ensures the pixel data is decompressed.
                var file = originalFile.Clone(DicomTransferSyntax.ExplicitVRLittleEndian);

                var dataSet = file.Dataset;
                var sopclass = dataSet.GetValueOrDefault(DicomTag.SOPClassUID, 0, string.Empty);

                if (
                    sopclass == DicomSopClasses.CTImageStorageSopClass ||
                    sopclass == DicomSopClasses.MRImageStorageSopClass ||
                    sopclass == DicomSopClasses.EnhancedMRImageStorageSopClass ||
                    sopclass == DicomSopClasses.XRayAngiographicImageStorageSopClass)
                {
                    var dicomPixelData = DicomPixelData.Create(dataSet);
                    var nrFrames = dicomPixelData.NumberOfFrames;

                    for (int i = 0; i < nrFrames; i++)
                    {
                        var pixelData = PixelDataFactory.Create(dicomPixelData, i);

                        ImageData image = CreateImageData(pixelData);
                        if (image == null) { continue; }
                        volume.Slices.Add(image);

                        GetPixelSpacing(dataSet, image);
                        GetPositionPatient(dataSet, image);

                        var dicomOrientationPatient = dataSet.GetDicomItem<DicomDecimalString>(DicomTag.ImageOrientationPatient);
                        GetImageOrientationPatient(image, dicomOrientationPatient);

                        image.Intercept = dataSet.GetValueOrDefault<double>(DicomTag.RescaleIntercept, 0, 0);
                        image.Slope = dataSet.GetValueOrDefault<double>(DicomTag.RescaleSlope, 0, 1);

                        var range = pixelData.GetMinMax();

                        double minValue = (image.Slope * range.Minimum) + image.Intercept;
                        double maxValue = (image.Slope * range.Maximum) + image.Intercept;
                        image.WindowLevel = (minValue + maxValue) / 2;
                        image.WindowWidth = maxValue - minValue;
                        image.DefaultWindowingAvailable = true;
                    }
                }
            }

            DeriveVolumeFields(volume);
            var patient = GetPatient(files.First().Dataset);

            return new Scan { Volume = volume, Patient = patient };
        }

        private static Patient GetPatient(DicomDataset dataSet)
        {
            var result = new Patient();

            string name = dataSet.GetSingleValueOrDefault(DicomTag.PatientName, string.Empty);
            var id = dataSet.GetSingleValueOrDefault(DicomTag.PatientID, string.Empty);
            var dateOfBirth = dataSet.GetSingleValueOrDefault(DicomTag.PatientBirthDate, string.Empty);
            result.ParseDicomPersonName(name);
            result.ParseDicomDateOfBirth(dateOfBirth);
            result.Id = id;

            return result;
        }

        private static Matrix ToRotationMatrix(Vector3 xAxis, Vector3 yAxis)
        {
            var m = new Matrix();
            var zAxis = xAxis.Cross(yAxis);
            m[0] = xAxis.X;
            m[1] = xAxis.Y;
            m[2] = xAxis.Z;
            m[3] = 0;

            m[4] = yAxis.X;
            m[5] = yAxis.Y;
            m[6] = yAxis.Z;
            m[7] = 0;

            m[8] = zAxis.X;
            m[9] = zAxis.Y;
            m[10] = zAxis.Z;
            m[11] = 0;

            m[12] = 0;
            m[13] = 0;
            m[14] = 0;
            m[15] = 1;
            return m;
        }

        private static ImageData CreateImageData(IPixelData pixelData)
        {
            ImageData image = null;

            if (pixelData.Components != 1) { return null; }

            if (pixelData is GrayscalePixelDataU16 grayscalePixelDataU16)
            {
                image = new ImageData(grayscalePixelDataU16.Data);
            }
            else if (pixelData is GrayscalePixelDataS16 grayscalePixelDataS16)
            {
                image = new ImageData(grayscalePixelDataS16.Data);
            }
            else if (pixelData is GrayscalePixelDataU8 grayscalePixelDataU8)
            {
                image = new ImageData(grayscalePixelDataU8.Data);
            }

            image.Width = pixelData.Width;
            image.Height = pixelData.Height;

            var minMax = pixelData.GetMinMax();
            image.MinValue = minMax.Minimum;
            image.MaxValue = minMax.Maximum;

            return image;
        }

        private static Spacing3 GetVoxelSpacing(ImageSet volume)
        {
            var slices = volume.Slices;
            var firstSlice = slices.First();
            var spacing = new Spacing3
            {
                X = firstSlice.PixelSpacing.X,
                Y = (volume.Slices.Last().PositionPatient - volume.Slices.First().PositionPatient).Length() / volume.Slices.Count,
                Z = firstSlice.PixelSpacing.Y
            };
            return spacing;
        }
    }
}