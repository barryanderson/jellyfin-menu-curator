"""Adds the plugin image to a release zip built by jprm.

Jellyfin shows a plugin's image when meta.json in the plugin folder has `imagePath`,
a file inside that folder. jprm does not write `imagePath`, so manual installs would
show no image. This adds the image file to the zip, sets `imagePath`, and rewrites the
zip's .md5sum file to match.

Installs from the plugin repository also work: Jellyfin saves the manifest's `imageUrl`
under its file name in the plugin folder, and skips the download when that file exists.

Usage: python scripts/add-plugin-image.py <plugin zip> <image file>
"""

import hashlib
import json
import os
import sys
import zipfile


def main(zip_path: str, image_path: str) -> None:
    image_name = os.path.basename(image_path)
    temp_path = zip_path + ".tmp"

    with zipfile.ZipFile(zip_path) as source, zipfile.ZipFile(temp_path, "w", zipfile.ZIP_DEFLATED) as target:
        names = source.namelist()
        if "meta.json" not in names:
            sys.exit(f"{zip_path} has no meta.json")

        for item in source.infolist():
            if item.filename == image_name:
                continue
            data = source.read(item.filename)
            if item.filename == "meta.json":
                meta = json.loads(data)
                meta["imagePath"] = image_name
                data = (json.dumps(meta, indent=4, ensure_ascii=False) + "\n").encode("utf-8")
            target.writestr(item, data)

        target.write(image_path, image_name)

    os.replace(temp_path, zip_path)

    with open(zip_path, "rb") as fh:
        md5 = hashlib.md5(fh.read()).hexdigest()
    with open(zip_path + ".md5sum", "w", encoding="utf-8", newline="\n") as fh:
        fh.write(f"{md5} *{os.path.basename(zip_path)}\n")

    print(f"Added {image_name} to {zip_path} (md5 {md5})")


if __name__ == "__main__":
    if len(sys.argv) != 3:
        sys.exit(__doc__)
    main(sys.argv[1], sys.argv[2])
