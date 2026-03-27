# RPGMakerCompressor

RPGMakerCompressor is a console-based application used to compress RPG Maker MV/MZ game assets.

It uses lossy compression with conservative parameters to reduce file size while minimizing noticeable visual and audio quality loss.

## Features

* Compresses image and audio assets
* Supports RPG Maker MV/MZ (Perhaps more in the future.)
* Designed for good compression with minimal quality loss

## Usage

1. Launch the application
2. On first launch, required dependencies will be downloaded automatically
3. Restart the application once setup is complete
4. Provide the path to your RPG Maker MV/MZ game

The application will:

* Decrypt the game files
* Compress image and audio assets
* Output the results to the `Output` folder upon completion

## Installing Compressed Assets

After compression:

1. Navigate to your game's `www` folder
2. Delete the original `img` and `audio` folders
3. Copy the compressed files from the `Output` folder
4. Overwrite files if prompted

## Dependencies

These dependencies are automatically downloaded on first launch:

* [RPGMakerDecrypter](https://github.com/Hexnet111/RPGMakerDecrypter) (fork) - Decrypting of RPG Maker game files
* [FFmpeg](https://github.com/BtbN/FFmpeg-Builds) — Audio processing
* [oxipng](https://github.com/oxipng/oxipng) — PNG optimization
* [pngquant](https://pngquant.org/) — Lossy PNG compression

## Notes

* Dependencies are downloaded as prebuilt binaries from their official sources
* No dependencies are bundled with the GitHub release

## License

This project is licensed under the GPL-3.0 license. See the `LICENSE` file for details.
