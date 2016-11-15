This is a converter for the `*.h` fonts from [jcw/GLCDlib][L] on GitHub.

* `fonts-h/` contains the original files from the `utility/` area in GLCDlib
* `fonts/` contains the `.h` files converted to go (by `h2go.sh`)
* `fonts-show/` files are ASCII art text files, viewable from the console
* `fonts-raw/` contains the binary version, extracted from the `.h` files

The main application is `fontconv.go` - it's built and used with this one-liner:

    go generate && go run fontconv.go

This produces the `fonts/`, `fonts-show/`, and `fonts-raw/` areas and contents.

   [L]: https://github.com/jcw/glcdlib
