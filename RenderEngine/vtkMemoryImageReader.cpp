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
{
    this->SetNumberOfInputPorts(0);
}


vtkMemoryImageReader::~vtkMemoryImageReader()
{
}

void vtkMemoryImageReader::SetImages(
    void ** images,
    int width,
    int height,
    int numberOfImages,
    double rescaleIntercept,
    double rescaleSlope,
    double pixelSpacingX,
    double pixelSpacingY)
{
    this->images = images;
    this->width = width;
    this->height = height;
    this->numberOfImages = numberOfImages;
    this->rescaleIntercept = rescaleIntercept;
    this->rescaleSlope = rescaleSlope;
    this->pixelSpacingX = pixelSpacingX;
    this->pixelSpacingY = pixelSpacingY;
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
    output->SetScalarType(VTK_SHORT, GetOutputInformation(0));
    //output->SetSpacing(pixelSpacingX, pixelSpacingY, 1);
    output->SetOrigin(-width / 2, -height / 2, -numberOfImages / 2);
    output->SetNumberOfScalarComponents(1, GetOutputInformation(0));
    output->AllocateScalars(VTK_SHORT, 1);
    output->SetExtent(0, width - 1, 0, height - 1, 0, numberOfImages - 1);

    short* p = (short*)output->GetScalarPointer();

    for (int y = 0; y < numberOfImages; y++)
    {
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                long offset = z * width + x;
                long storedValue;
                if (false) //isSigned
                {
                    storedValue = ((long) *((short*)images[y] + offset));
                }
                else
                {
                    storedValue = ((long) *((unsigned short*)images[y] + offset));
                }
                *p++ = (short)(rescaleSlope * storedValue) + rescaleIntercept;
            }
        }
    }
    output->Modified();
}