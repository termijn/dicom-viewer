#include "vtkDicomVolumeImageReader.h"
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


vtkStandardNewMacro(vtkVolumeMemoryReader);

vtkVolumeMemoryReader::vtkVolumeMemoryReader()
{
    this->SetNumberOfInputPorts(0);
}

vtkVolumeMemoryReader::~vtkVolumeMemoryReader()
{
}

void vtkVolumeMemoryReader::SetSlices(
    void ** slices,
    int xSize,
    int ySize,
    int zSize,
    double xSpacing,
    double ySpacing,
    double zSpacing,
    bool isSigned,
    double rescaleIntercept,
    double rescaleSlope)
{
    this->slices = slices;
    this->xSize = xSize;
    this->ySize = ySize;
    this->zSize = zSize;
    this->isSigned = isSigned;
    this->rescaleIntercept = rescaleIntercept;
    this->rescaleSlope = rescaleSlope;
    this->xSpacing = xSpacing;
    this->ySpacing = ySpacing;
    this->zSpacing = zSpacing;
}

int vtkVolumeMemoryReader::RequestUpdateExtent(vtkInformation *, vtkInformationVector ** inputVector, vtkInformationVector * outputVector)
{
    return 1;
}

int vtkVolumeMemoryReader::RequestData(vtkInformation *, vtkInformationVector **, vtkInformationVector * outputVector)
{
    // get the data object
    vtkInformation *outInfo = outputVector->GetInformationObject(0);
    vtkImageData *output = vtkImageData::SafeDownCast(
        outInfo->Get(vtkDataObject::DATA_OBJECT()));

    this->Execute(output);

    return 1;
}

void vtkVolumeMemoryReader::Execute(vtkImageData * output)
{
    output->SetDimensions(xSize, zSize, ySize);
    output->SetScalarType(VTK_SHORT, GetOutputInformation(0));
    output->SetSpacing(xSpacing, zSpacing, ySpacing);
    output->SetOrigin(0, 0, 0);
    output->SetNumberOfScalarComponents(1, GetOutputInformation(0));
    output->AllocateScalars(GetOutputInformation(0));

    short* p = (short*)output->GetScalarPointer();

    for (int y = 0; y < ySize; y++)
    {
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                long offset = z * xSize + x;
                long storedValue;
                if (isSigned)
                {
                    storedValue = ((long) *((short*)slices[y] + offset));
                }
                else
                {
                    storedValue = ((long) *((unsigned short*)slices[y] + offset));
                }
                *p++ = (short)(rescaleSlope * storedValue) + rescaleIntercept;
            }
        }
    }
}
