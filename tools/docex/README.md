This is a documentation expander for Embello. It parses a Markdown file for
_directives_ and inserts word definitions copied from source files referenced in
those directives.

**Usage: `docex [-d <dir>] docfile...`**

* _dir_ is the root directory where source files are read from (default ".")
* expand from stdin to stdout if no docfiles are passed in)

The directives are in a format which will be ignored by Markdown:

* `"[code]:" <source-file> "(" <dependency-list> ")"`

    This needs to be the first directive in the documentation file, it loads the
    specified _source-file_, and generates a few documentation lines. Example:

        [code]: spi/rf69.fs (spi)
      
* `"[defs]:" <source-file> "(" <word-list> ")"`

    Insert word definitions found in the source code, in the order
    specified in the _word-list_. The _source-file_ can be `<>` if it's the same as
    in the previous directive. Example:

        [defs]: <> (rf-init rf-recv rf-send)

**WARNING #1:** be sure to always add an _empty line_ after each directive,
because the expander will _replace_ everything up to that next empty line with
updated information.

**WARNING #2:** all files passed as argument will be _overwritten_ by their
expanded versions.

To update the documentation at a later date, run the expander again on the same
files. Due to the way it is set up, all existing expansions will be replaced by
updated ones.
