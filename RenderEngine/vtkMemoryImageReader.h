#pragma once

#include <vtkSimpleImageToImageFilter.h>

class vtkMemoryImageReader : public vtkImageAlgorithm
{
public:
    static vtkMemoryImageReader *New();
    vtkTypeMacro(vtkMemoryImageReader, vtkImageAlgorithm);

    vtkMemoryImageReader();
    ~vtkMemoryImageReader();

    void SetImages(
        void ** images,
		int bytesPerPixel,
        int width,
        int height,
        int numberOfImages,
        double rescaleIntercept,
        double rescaleSlope,
        double pixelSpacingX,
        double pixelSpacingY);

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

	template <typename T>
	void Copy(T** slices, T* destination);

    void** images;
	int bytesPerPixel;
    int width;
    int height;
    int numberOfImages;
    double rescaleIntercept;
    double rescaleSlope;
    double pixelSpacingX;
    double pixelSpacingY;
};