# msend

This utility is intended for "picocom" in combination with Mecrisp Forth.
It sends out text lines from a file without over-running the input buffer.
Relies on the fact that processing starts after a newline has been sent.

Msend does this by waiting up to 500 milliseconds for new input to arrive.
The trick is that as soon as a received text line matches what was just
sent out plus an "ok." prompt at the end, then it immediately moves on to
the next line. This allows sending source code lines at maximum speed.

* **include** _filename_  
    Include directives can be used to insert another source file.

* **require** _filename_  
    Similar to include, but this won't re-include a file if already sent.

To reduce clutter, the exact-echo lines are also not passed on to picocom.
Only lines which are not precisely the same as the input will be shown.
Comment lines starting with "\" and empty lines are not sent.

If there's a "not found" error, it will be shown and abort the upload.
