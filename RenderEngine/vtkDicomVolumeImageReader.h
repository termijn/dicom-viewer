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
    int xSize = 0;
    int ySize = 0;
    int zSize = 0;
	bool isSigned = false;
    double rescaleIntercept = 0;
    double rescaleSlope = 1;
	double xSpacing = 1;
	double ySpacing = 1;
	double zSpacing = 1;
};