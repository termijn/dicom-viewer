#pragma once

#include "vtkMemoryImageReader.h"

namespace RenderEngine {

	ref class ImageSetReaderItem 
	{
	public:
		Entities::ImageSet^ ImageSet;
		vtkMemoryImageReader* Reader;
		int RefCount = 0;
	};

	ref class ImageSetReaderFactory
	{
	public:
		
		static vtkMemoryImageReader* Acquire(Entities::ImageSet^ imageSet) 
		{
			ImageSetReaderItem^ imageSetReaderItem = nullptr;

			for each(auto reader in readers)
			{
				if (reader->ImageSet == imageSet)
				{
					imageSetReaderItem = reader;
					break;
				}
			}

			if (imageSetReaderItem == nullptr)
			{
				imageSetReaderItem = gcnew ImageSetReaderItem();
				imageSetReaderItem->ImageSet = imageSet;
				imageSetReaderItem->Reader = vtkMemoryImageReader::New();

				int numberOfImages = imageSet->Slices->Count;
				int width = imageSet->Slices[0]->Width;
				int height = imageSet->Slices[0]->Height;

				void** pixels = new void*[imageSet->Slices->Count];
				int i = 0;
				for each(auto image in imageSet->Slices)
				{
					pixels[i++] = image->Pixels.ToPointer();
				}

				auto firstImage = imageSet->Slices[0];

				imageSetReaderItem->Reader->SetImages(
					pixels, 
					firstImage->BytesPerPixel, 
					width, 
					height, 
					imageSet->Slices->Count, 
					firstImage->Intercept, 
					firstImage->Slope, 
					imageSet->VoxelSpacing->X,
					imageSet->VoxelSpacing->Y,
					imageSet->VoxelSpacing->Z,					
					imageSet->Slices[0]->IsSigned);
				imageSetReaderItem->Reader->Update();

				readers->Add(imageSetReaderItem);			
			}
			imageSetReaderItem->RefCount++;
			return imageSetReaderItem->Reader;
		}

		static void Release(Entities::ImageSet^ imageSet)
		{
			ImageSetReaderItem^ imageSetReaderItem = nullptr;

			for each(auto reader in readers)
			{
				if (reader->ImageSet == imageSet)
				{
					imageSetReaderItem = reader;
					
					break;
				}
			}

			if (imageSetReaderItem != nullptr)
			{
				imageSetReaderItem->RefCount--;
				if (imageSetReaderItem->RefCount == 0)
				{
					imageSetReaderItem->Reader->Delete();
					readers->Remove(imageSetReaderItem);
				}
			}
		}
		
	private:
		
		static System::Collections::Generic::List<ImageSetReaderItem^>^ readers = gcnew System::Collections::Generic::List<ImageSetReaderItem^>();

	};

}