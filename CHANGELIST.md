# Riverscapes Viewer for ArcPro

## Known issues

- If you drag layers or group layers out of order, or indeed out of the parent project group layer then continue to more layers to the map, you will layers added to the map in unexpected order.

## [1.0.7] 7 Apr 2025

- Adding GeoPackage layers using database connection. Fixes issue with layer extents not being detected correctly for GeoPackage layers.

## [1.0.6] 11 Mar 2025

- Further improvements to layer order when adding items to the map.
- Now attempts to zoom to project when adding first layer to the map.

## [1.0.5] 7 Mar 2025

- Release only for ArcPro 3.3 and higher.
- Fixed map ToC groups being out of order.
- Fixed generation of multiple new maps when using "Empty Template".
- Finished Acknowledgements Screen.
- Fixed Project Explorer button triggering wrong action.

## [1.0.4a] 8 Nov 2024

### Added
- Creates a new map if one doesn't exist when adding layers to map.
- Dedicated Riverscapes tab instead of adding controls to AddIn tab in ArcPro.

### Fixed
- Correctly looking in `arcpro` folder for symbology files.
- Fixed expanding gorups in project tree instead of being collapsed.

## [1.0.3a] 7 Nov 2024

### Changed

- Reverted to ArcPro 3.1 and .net 6.

### Fixed

- Ghost layers when adding layers to the map.