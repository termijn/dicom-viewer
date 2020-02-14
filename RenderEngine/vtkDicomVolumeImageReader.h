#pragma once

#include <vtkSimpleImageToImageFilter.h>

class vtkVolumeMemoryReader : public vtkImageAlgorithm
{
public:
	static vtkVolumeMemoryReader *New();
	vtkTypeMacro(vtkVolumeMemoryReader, vtkImageAlgorithm);

    vtkVolumeMemoryReader();
	~vtkVolumeMemoryReader();

    void SetSlices(
		void ** slices, 
		int xSize, 
		int ySize, 
		int zSize, 
		double xSpacing,
		double ySpacing,
		double zSpacing, 
		bool isSigned, 
		double rescaleIntercept, 
		double rescaleSlope);

	int RequestUpdateExtent(vtkInformation *,
		vtkInformationVector **,
		vtkInformationVector *) override;

	int RequestData(vtkInformation *,
		vtkInformationVector **,
		vtkInformationVector *) override;

	virtual void Execute(vtkImageData* output);

private:
    vtkVolumeMemoryReader(const vtkVolumeMemoryReader&) = delete;
	void operator=(const vtkVolumeMemoryReader&) = delete;

    void ** slices;
    int xSize;
    int ySize;
    int zSize;
    bool isSigned;
    double rescaleIntercept;
    double rescaleSlope;
	double xSpacing;
	double ySpacing;
	double zSpacing;
};