#include "vtkMemoryImageReader.h"

#include "vtkImageData.h"

#include "vtkByteSwap.h"
#include "vtkDataArray.h"
#include "vtkImageData.h"
#include "vtkInformation.h"
#include "vtkInformationVector.h"
#include "vtkObjectFactory.h"
#include "vtkPointData.h"
#include "vtkErrorCode.h"
#include "vtkStringArray.h"
#include <vtkStreamingDemandDrivenPipeline.h>
#include "vtksys/SystemTools.hxx"

vtkStandardNewMacro(vtkMemoryImageReader);

vtkMemoryImageReader::vtkMemoryImageReader()
    : images(nullptr)
    , bytesPerPixel(1)
    , width(0)
    , height(0)
    , numberOfImages(0)
    , rescaleIntercept(0)
    , rescaleSlope(1)
    , pixelSpacingX(1)
    , pixelSpacingY(1)
    , pixelSpacingZ(1)
    , isSigned(false)
{
    this->SetNumberOfInputPorts(0);
}


vtkMemoryImageReader::~vtkMemoryImageReader()
{
}

void vtkMemoryImageReader::SetImages(
    void ** images,
    int bytesPerPixel,
    int width,
    int height,
    int numberOfImages,
    double rescaleIntercept,
    double rescaleSlope,
    double pixelSpacingX,
    double pixelSpacingY,
    double pixelSpacingZ,
    bool isSigned)
{
    this->images = images;
    this->bytesPerPixel = bytesPerPixel;
    this->width = width;
    this->height = height;
    this->numberOfImages = numberOfImages;
    this->rescaleIntercept = rescaleIntercept;
    this->rescaleSlope = rescaleSlope;
    this->pixelSpacingX = pixelSpacingX;
    this->pixelSpacingY = pixelSpacingY;
    this->pixelSpacingZ = pixelSpacingZ;
    this->isSigned = isSigned;
}

int vtkMemoryImageReader::RequestUpdateExtent(vtkInformation *, vtkInformationVector ** inputVector, vtkInformationVector * outputVector)
{
    return 1;
}

int vtkMemoryImageReader::RequestData(vtkInformation *, vtkInformationVector **, vtkInformationVector * outputVector)
{
    vtkInformation *outInfo = outputVector->GetInformationObject(0);
    vtkImageData *output = vtkImageData::SafeDownCast(
        outInfo->Get(vtkDataObject::DATA_OBJECT()));

    this->Execute(output);

    return 1;
}

void vtkMemoryImageReader::Execute(vtkImageData * output)
{
    output->SetDimensions(width, height, numberOfImages);
    output->SetSpacing(pixelSpacingX, pixelSpacingY, pixelSpacingZ);
    output->SetOrigin(0, 0, 0);
    output->SetNumberOfScalarComponents(1, GetOutputInformation(0));
    output->SetExtent(0, width - 1, 0, height - 1, 0, numberOfImages - 1);
    output->SetScalarType(VTK_SHORT, GetOutputInformation(0));
    output->AllocateScalars(VTK_SHORT, 1);

    short* p = (short*)output->GetScalarPointer();
    if (bytesPerPixel == 2 && !isSigned)
    {
        Copy<unsigned short>((unsigned short**)images, p);
    }
    else if (bytesPerPixel == 1 && !isSigned)
    {
        Copy<unsigned char>((unsigned char**)images, p);
    }
    else if (bytesPerPixel == 2 && isSigned)
    {
        Copy<short>((short**)images, p);
    }
    else if (bytesPerPixel == 1 && isSigned)
    {
        Copy<char>((char**)images, p);
    }

    output->Modified();
}

template<typename T>
inline void vtkMemoryImageReader::Copy(T ** slices, short * destination)
{
    short* p = destination;
    for (int y = 0; y < numberOfImages; y++)
    {
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                long offset = z * width + x;
                long storedValue;
                storedValue = ((long) *((T*)images[y] + offset));
                *p++ = (short)(rescaleSlope * storedValue) + rescaleIntercept;
            }
        }
    }
}
