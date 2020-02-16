#pragma once

#include <vtkSimpleImageToImageFilter.h>

class vtkMemoryImageReader : public vtkImageAlgorithm
{
public:
	static vtkMemoryImageReader *New();
	vtkTypeMacro(vtkMemoryImageReader, vtkImageAlgorithm);

	vtkMemoryImageReader();
	~vtkMemoryImageReader();

	void SetImages(void ** images, int width, int height, int numberOfImages);

	int RequestUpdateExtent(vtkInformation *,
		vtkInformationVector **,
		vtkInformationVector *) override;

	int RequestData(vtkInformation *,
		vtkInformationVector **,
		vtkInformationVector *) override;

	virtual void Execute(vtkImageData* output);

private:
	vtkMemoryImageReader(const vtkMemoryImageReader&) = delete;
	void operator=(const vtkMemoryImageReader&) = delete;

	void** images;
	int width;
	int height; 
	int numberOfImages;
};

