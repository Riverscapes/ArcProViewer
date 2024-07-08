# ArcProViewer

This repository contains the Riverscapes Viewer for ESRI's ArcGIS Pro. This is an addin for ArcGIS that provides the ability to open and view Riverscapes compliant projects. 

ArcProViewer replaces the old [ArcViewer](https://viewer.riverscapes.net/software-help/help-arc/) ArcGIS 10.x for ArcMap version, and provides the same functionality as the [QViewer](https://viewer.riverscapes.net/software-help/help-qgis/) for QGIS.

# Documentation

See the [Riverscapes Viewer](https://viewer.riverscapes.net/) site.

# Minimum Requirements

[ArcGIS Pro](https://www.esri.com/en-us/arcgis/products/arcgis-pro/overview) version 3.3.

# License

[GNU GENERAL PUBLIC LICENSE](https://raw.githubusercontent.com/Riverscapes/ArcProViewer/main/LICENSE)

# Development

ArcProViewer is written in c# and developed with Visual Studio 2022. It uses [WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/overview/?view=netdesktop-8.0) (as opposed to the old WinForms and the newer WinUI user interface technologies.)

Configuring a build environment requires that you follow the steps on ESRI's [build my first addin for ArcPro](https://developers.arcgis.com/documentation/arcgis-pro-sdk/tutorials/build-your-first-add-in/). The key step in this setup is to [install the ArGIS Pro SDK for .Net](https://github.com/Esri/arcgis-pro-sdk/wiki/ProGuide-Installation-and-Upgrade). This is performed inside Visual Studio itself using the Extension Manager (as opposed to running a standalone installer).

![Pro SDK](https://camo.githubusercontent.com/e6430218e74635d78e78a6b700eaf37e1bec9059783db0b1b2977927b5e327cf/68747470733a2f2f457372692e6769746875622e696f2f6172636769732d70726f2d73646b2f696d616765732f496e7374616c6c6174696f6e2d496e737472756374696f6e732f4f6e6c696e655f4f7074696f6e312e706e67)

# Deployment

1. Increment the version number.
1. Commit and push all your changes. Also pull to ensure you have the latest code.
1. Create a tag for the release that matches the version number. Commit and push this tag to GitHub.
1. Switch to Release build.
1. Rebuild the software solution.
1. Navigate to the build folder.
1. Create a new Release on the GitHub repository [releases](https://github.com/Riverscapes/ArcProViewer/releases) that references the new tag.
1. Provide a name and description and drag the `ArcProViewer.esriAddinX` file into the binaries section.
1. Save the release.
1. Announce the release on the [Riverscapes Consortium](https://www.riverscapes.net/feed) community platform.

# Notes

- Does NOT ship with a copy of business logic or symbology. Users must synchronize this manually after installation.
- Uses *.lyvrx files symbology. It is incompatible with the *.lyr files uses by ArcViewer for ArcMap 10.x.
- Does not possess an [upload](https://viewer.riverscapes.net/software-help/help-qgis-uploader/) feature for uploading projects to the [Riverscapes Data Exchange](https://data.riverscapes.net/).