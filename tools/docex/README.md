This is a documentation expander for Embello. It parses a Markdown file for
_directives_ and inserts word definitions copied from source files referenced in
those directives.

The directives are in a format which will be ignored by Markdown:

* `"[code]:" <source-file> "(" <dependency-list> ")"`

    This needs to be the first directive in the documentation file, it loads the
    specified _source file_, and generates a few documentation lines. Example:

      [code]: rf69.fs (spi)
      
* `"[defs]:" <source-file> "(" <word-list> ")"`

    This inserts word definitions found in the source code, in the order
    specified in the _word list_. _Source file_ can be "<>" if it's the same as
    in the previous directive. Example:

      [defs]: <> (rf-init rf-recv rf-send)

Usage: `docex docfile...`

> Note that these files will be _overwritten_ by their expanded versions, and
> that the documentation files can be updated by running docex again.
