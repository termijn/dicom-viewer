# Dicom Viewer

[![Codacy Badge](https://api.codacy.com/project/badge/Grade/8b30fd5523f9450da9e95f2d4695ec10)](https://app.codacy.com/manual/martijn_5/dicom-viewer?utm_source=github.com&utm_medium=referral&utm_content=termijn/dicom-viewer&utm_campaign=Badge_Grade_Dashboard)

![DICOM viewer](.github/screenshot.png)

* Supported DICOM classes
    * CTImageStorageSopClass - 1.2.840.10008.5.1.4.1.1.2
    * MRImageStorageSopClass - 1.2.840.10008.5.1.4.1.1.4
    * EnhancedMRImageStorageSopClass - 1.2.840.10008.5.1.4.1.1.4.1
    * XA3DImageStorageSopClass - 1.2.840.10008.5.1.4.1.1.13.1.1

* Build
    * Build using Visual Studio 2017 or newer.
    * Uses VTK for visualization.
    * Install VTK toolkit and set VTK_PATH environment variable to installed location (e.g. VTK_PATH=C:\Program Files\VTK).
    * Other dependencies are automatically installed from nuget.org upon first build. 

