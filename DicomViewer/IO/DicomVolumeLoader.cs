using Dicom;
using Dicom.Imaging;
using Dicom.Imaging.Codec;
using Dicom.Imaging.Render;
using Entities;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DicomViewer.IO
{

    public class DicomVolumeLoader
    {
        private const string CTImageStorageSopClass = "1.2.840.10008.5.1.4.1.1.2";
        private const string MRImageStorageSopClass = "1.2.840.10008.5.1.4.1.1.4";
        private const string EnhancedMRImageStorageSopClass = "1.2.840.10008.5.1.4.1.1.4.1";
        private const string XA3DImageStorageSopClass = "1.2.840.10008.5.1.4.1.1.13.1.1";

        public Scan Load(string path)
        {
            if (Directory.Exists(path))
            {
                var files = Directory.EnumerateFiles(path);
                var dicomFiles = files.Where(file => DicomFile.HasValidHeader(file));

                var filesPerSeries = GetFilesPerSeries(dicomFiles);
                if (filesPerSeries.Count == 0) return null;

                if (filesPerSeries.Values.First().Count == 1)
                {
                    return Load(filesPerSeries.FirstOrDefault().Value.FirstOrDefault());
                }
                else
                {
                    var kvp = filesPerSeries.FirstOrDefault();
                    var dicomFilesForFirstSeries = kvp.Value;

                    return Load(dicomFilesForFirstSeries);
                }
            }
            else if (File.Exists(path))
            {
                return Load(DicomFile.Open(path, FileReadOption.ReadLargeOnDemand));
            }
            return null;
        }

        public Scan Load(DicomSeries series)
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
                foreach(var fileName in series.FileNames)
                {
                    var file = DicomFile.Open(fileName);
                    files.Add(file);
                }
                return Load(files);
            }
            return null;
        }

        private Scan LoadXA3DImageStorage(DicomFile dicomFile)
        {
            var volume = new VolumeData();
            var dataSet = dicomFile.Dataset;

            var dicomPixelData = DicomPixelData.Create(dataSet);
            var nrFrames = dicomPixelData.NumberOfFrames;

            var functionalGroupShared = dataSet.GetSequence(DicomTag.SharedFunctionalGroupsSequence);
            var frameDataSet = functionalGroupShared.ElementAt(0);

            for (int i = 0; i < nrFrames; i++)
            {
                var pixelData = PixelDataFactory.Create(dicomPixelData, i);

                if (pixelData.Components != 1) continue;

                ImageData image = null;
                if (pixelData is GrayscalePixelDataU16 grayscalePixelDataU16)
                {
                    image = new ImageData(grayscalePixelDataU16.Data);
                }
                else if (pixelData is GrayscalePixelDataS16 grayscalePixelDataS16)
                {
                    image = new ImageData(grayscalePixelDataS16.Data);
                }
                else
                {
                    // Unsupported data format
                    continue;
                }

                image.Width = dicomPixelData.Width;
                image.Height = dicomPixelData.Height;
                volume.Slices.Add(image);

                var pixelMeasures = frameDataSet.GetSequence(DicomTag.PixelMeasuresSequence);
                var pixelMeasuresDataSet = pixelMeasures.ElementAt(0);
                var dicomPixelSpacing = pixelMeasuresDataSet.GetDicomItem<DicomDecimalString>(DicomTag.PixelSpacing);
                image.PixelSpacing.X = dicomPixelSpacing.Get<double>(0);
                image.PixelSpacing.Y = dicomPixelSpacing.Get<double>(1);

                var planeOrientations = frameDataSet.GetSequence(DicomTag.PlaneOrientationSequence);
                var planeOrientationDataSet = planeOrientations.ElementAt(0);
                var dicomOrientationPatient = planeOrientationDataSet.GetDicomItem<DicomDecimalString>(DicomTag.ImageOrientationPatient);
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

                var functionalGroupPerFrame = dataSet.GetSequence(DicomTag.PerFrameFunctionalGroupsSequence);
                var functionalGroupPerFrameDataSet = functionalGroupPerFrame.ElementAt(i);

                var planePositionSequence = functionalGroupPerFrameDataSet.GetSequence(DicomTag.PlanePositionSequence);
                var planePositionDataSet = planePositionSequence.ElementAt(0);
                var dicomPositionPatient = planePositionDataSet.GetDicomItem<DicomDecimalString>(DicomTag.ImagePositionPatient);
                image.PositionPatient.X = dicomPositionPatient.Get<double>(0);
                image.PositionPatient.Y = dicomPositionPatient.Get<double>(1);
                image.PositionPatient.Z = dicomPositionPatient.Get<double>(2);

                ReadWindowing(image, functionalGroupPerFrameDataSet);
                ReadPixelTransformation(image, frameDataSet, functionalGroupPerFrameDataSet);
            }

            var firstDataset = dataSet;
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

            var patient = GetPatient(dataSet);

            return new Scan { Volume = volume, Patient = patient };
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

        private Scan Load(DicomFile dicomFile)
        {
            var file = dicomFile.Clone(DicomTransferSyntax.ExplicitVRLittleEndian);
            var dataSet = file.Dataset;
            var sopclass = dataSet.GetValueOrDefault(DicomTag.SOPClassUID, 0, string.Empty);

            if (sopclass == XA3DImageStorageSopClass)
            {
                return LoadXA3DImageStorage(dicomFile);
            }
            else if (sopclass == EnhancedMRImageStorageSopClass)
            {
                return LoadEnhancedMRImage(dicomFile);
            }
            return null;
        }

        private Scan LoadEnhancedMRImage(DicomFile dicomFile)
        {
            var volume = new VolumeData();
            var dataSet = dicomFile.Dataset;

            var dicomPixelData = DicomPixelData.Create(dataSet);
            var nrFrames = dicomPixelData.NumberOfFrames;

            var functionalGroupShared = dataSet.GetSequence(DicomTag.SharedFunctionalGroupsSequence);
            var frameDataSet = functionalGroupShared.ElementAt(0);
            DicomDataset functionalGroupPerFrameDataSet = null;
            for (int i = 0; i < nrFrames; i++)
            {
                var pixelData = PixelDataFactory.Create(dicomPixelData, i);

                if (pixelData.Components != 1) continue;

                ImageData image = null;
                if (pixelData is GrayscalePixelDataU16 grayscalePixelDataU16)
                {
                    image = new ImageData(grayscalePixelDataU16.Data);
                }
                else if (pixelData is GrayscalePixelDataS16 grayscalePixelDataS16)
                {
                    image = new ImageData(grayscalePixelDataS16.Data);
                }
                else
                {
                    // Unsupported data format
                    continue;
                }

                image.Width = dicomPixelData.Width;
                image.Height = dicomPixelData.Height;
                volume.Slices.Add(image);

                var functionalGroupPerFrame = dataSet.GetSequence(DicomTag.PerFrameFunctionalGroupsSequence);
                functionalGroupPerFrameDataSet = functionalGroupPerFrame.ElementAt(i);
                var pixelMeasureSequence = functionalGroupPerFrameDataSet.GetSequence(DicomTag.PixelMeasuresSequence);
                var pixelMeasureSequenceItem = pixelMeasureSequence.FirstOrDefault();

                var dicomPixelSpacing = pixelMeasureSequenceItem.GetDicomItem<DicomDecimalString>(DicomTag.PixelSpacing);
                image.PixelSpacing.X = dicomPixelSpacing.Get<double>(0);
                image.PixelSpacing.Y = dicomPixelSpacing.Get<double>(1);

                var planeOrientations = functionalGroupPerFrameDataSet.GetSequence(DicomTag.PlaneOrientationSequence);
                var planeOrientationDataSet = planeOrientations.ElementAt(0);
                var dicomOrientationPatient = planeOrientationDataSet.GetDicomItem<DicomDecimalString>(DicomTag.ImageOrientationPatient);
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

                var planePositionSequence = functionalGroupPerFrameDataSet.GetSequence(DicomTag.PlanePositionSequence);
                var planePositionDataSet = planePositionSequence.ElementAt(0);
                var dicomPositionPatient = planePositionDataSet.GetDicomItem<DicomDecimalString>(DicomTag.ImagePositionPatient);
                image.PositionPatient.X = dicomPositionPatient.Get<double>(0);
                image.PositionPatient.Y = dicomPositionPatient.Get<double>(1);
                image.PositionPatient.Z = dicomPositionPatient.Get<double>(2);

                ReadWindowing(image, functionalGroupPerFrameDataSet);
                ReadPixelTransformation(image, frameDataSet, functionalGroupPerFrameDataSet);
            }

            var firstDataset = dataSet;
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

            var patient = GetPatient(dataSet);

            return new Scan { Volume = volume, Patient = patient };
        }

        private Scan Load(List<DicomFile> files)
        {
            if (files.Count < 2) return null;

            var volume = new VolumeData();

            var spacing = new double[3];

            foreach (var originalFile in files)
            {
                // This ensures the pixel data is decompressed.
                var file = originalFile.Clone(DicomTransferSyntax.ExplicitVRLittleEndian);

                var dataSet = file.Dataset;
                var sopclass = dataSet.GetValueOrDefault(DicomTag.SOPClassUID, 0, string.Empty);

                if (
                    sopclass == CTImageStorageSopClass ||
                    sopclass == MRImageStorageSopClass ||
                    sopclass == EnhancedMRImageStorageSopClass)
                {
                    var dicomPixelData = DicomPixelData.Create(dataSet);
                    var nrFrames = dicomPixelData.NumberOfFrames;

                    for (int i = 0; i < nrFrames; i++)
                    {
                        var pixelData = PixelDataFactory.Create(dicomPixelData, i);

                        if (pixelData.Components != 1) continue;

                        ImageData image = null;
                        if (pixelData is GrayscalePixelDataU16 grayscalePixelDataU16)
                        {
                            image = new ImageData(grayscalePixelDataU16.Data);
                        }
                        else if (pixelData is GrayscalePixelDataS16 grayscalePixelDataS16)
                        {
                            image = new ImageData(grayscalePixelDataS16.Data);
                        }
                        else
                        {
                            // Unsupported data format
                            continue;
                        }

                        image.Width = dicomPixelData.Width;
                        image.Height = dicomPixelData.Height;
                        volume.Slices.Add(image);

                        var dicomPixelSpacing = dataSet.GetDicomItem<DicomDecimalString>(DicomTag.PixelSpacing);
                        image.PixelSpacing.X = dicomPixelSpacing.Get<double>(0);
                        image.PixelSpacing.Y = dicomPixelSpacing.Get<double>(1);

                        var dicomPositionPatient = dataSet.GetDicomItem<DicomDecimalString>(DicomTag.ImagePositionPatient);
                        image.PositionPatient.X = dicomPositionPatient.Get<double>(0);
                        image.PositionPatient.Y = dicomPositionPatient.Get<double>(1);
                        image.PositionPatient.Z = dicomPositionPatient.Get<double>(2);

                        var dicomOrientationPatient = dataSet.GetDicomItem<DicomDecimalString>(DicomTag.ImageOrientationPatient);
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
                        image.Intercept = dataSet.GetValue<double>(DicomTag.RescaleIntercept, 0);
                        image.Slope = dataSet.GetValue<double>(DicomTag.RescaleSlope, 0);

                        var range = pixelData.GetMinMax();

                        double minValue = (image.Slope * range.Minimum) + image.Intercept;
                        double maxValue = (image.Slope * range.Maximum) + image.Intercept;
                        image.WindowLevel = (minValue + maxValue) / 2;
                        image.WindowWidth = maxValue - minValue;
                        image.DefaultWindowingAvailable = true;
                    }
                }
            }

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

            var patient = GetPatient(files.First().Dataset);

            return new Scan { Volume = volume, Patient = patient };
        }

        private Patient GetPatient(DicomDataset dataSet)
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

        private Matrix ToRotationMatrix(Vector3 xAxis, Vector3 yAxis)
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

        private Spacing3 GetVoxelSpacing(VolumeData volume)
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

        private Dictionary<string, List<DicomFile>> GetFilesPerSeries(IEnumerable<string> dicomFilePaths)
        {
            var seriesToFile = new Dictionary<string, List<DicomFile>>();
            foreach (var dicomFile in dicomFilePaths)
            {
                var file = DicomFile.Open(dicomFile);
                string seriesInstanceUid = file.Dataset.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, string.Empty);
                if (!string.IsNullOrEmpty(seriesInstanceUid))
                {
                    if (!seriesToFile.ContainsKey(seriesInstanceUid))
                    {
                        seriesToFile[seriesInstanceUid] = new List<DicomFile>();
                    }
                    seriesToFile[seriesInstanceUid].Add(file);
                }
            }
            return seriesToFile;
        }
    }
}
