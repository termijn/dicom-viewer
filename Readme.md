# Dicom Viewer

![DICOM viewer](.github/screenshot.png)

* Supported DICOM classes
    * CTImageStorageSopClass - 1.2.840.10008.5.1.4.1.1.2
    * MRImageStorageSopClass - 1.2.840.10008.5.1.4.1.1.4
    * EnhancedMRImageStorageSopClass - 1.2.840.10008.5.1.4.1.1.4.1
    * XA3DImageStorageSopClass - 1.2.840.10008.5.1.4.1.1.13.1.1

* Build
    * Uses VTK for visualization.
    * Install VTK toolkit and set VTK_PATH environment variable to installed location (e.g. VTK_PATH=C:\Program Files\VTK).
    * Other dependencies are automatically installed from nuget.org upon first build. 

