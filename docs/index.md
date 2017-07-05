The **embello** repository on [GitHub](https://github.com/jeelabs/embello/)
collects the files of most of the projects mentioned on the [JeeLabs
weblog](http://jeelabs.org/) since mid-2015, and various bits and pieces needed
to make them work. It's a hodgepodge of code, snippets, libraries, and
documents, because the weblog covers such a wide range of topics - it has always
done so, and probably always will.

### explore/

The [explore/](https://github.com/jeelabs/embello/tree/master/explore/) folder
contains most of the files. Its subdirectories are named as follows:

    <last-two-digits-of-year> <week-number> "-" <some-tag>

This corresponds to the weblog article series, which use a similar 4-digit
naming convention.

The
[explore/1608-forth/](https://github.com/jeelabs/embello/tree/master/explore/1608-forth/)
area was started in February 2016, but has been in use for a very long time
since, accumulating more and more Forth-related code. There's a reasonably
complete descripion of all its subdirectories in the
[explore/1608-forth/README.md](https://github.com/jeelabs/embello/tree/master/explore/1608-forth/README.md)
file.

The
[explore/1608-forth/flib/](https://github.com/jeelabs/embello/tree/master/explore/1608-forth/flib/)
area is structured as a _library_, with its files included in numerous other
subdirectories. See the [documentation area](/flib/).

### lib/

The [lib/](https://github.com/jeelabs/embello/tree/master/lib/) subdirectory is
a bit older, and was intended to play a similar role as `flib/`, but for the
C/C++ code written for NXP's LPC810 and LPC8xx µCs.

### projects/

The [projects/](https://github.com/jeelabs/embello/tree/master/projects/)
directory holds a variety of (older) projects, both software- and
hardware-related. See the
[projects/README.md](https://github.com/jeelabs/embello/tree/master/projects/README.md)
file for a brief list.

### docs/

The [docs/](https://github.com/jeelabs/embello/tree/master/docs/) tree
contains the source code for the documentation you are reading.

### tools/

The [tools/](https://github.com/jeelabs/embello/tree/master/tools/) area
contains the source code for a number of tools, also described on the weblog.
See the respective `README.md` files in each subdirectory for details.

## More links

For all background info and articles, see the weblog at <http://jeelabs.org/>.

For reporting problems and bugs, please use the [issue
tracker](https://github.com/jeelabs/embello/issues) on GitHub.

For general discussion and help, there's a forum at <http://jeelabs.net/>.  You
have to [register](http://jeelabs.net/account/register) and jump through a few
[hoops](http://jeelabs.net/boards/11/topics/5690) to be able to post and
participate in the discussions, due to some anti-spam measures - _but as a
result, the forum is more or less noise-free._

Everything in the embello repository is Open Source, see the
"[unlicense](https://github.com/jeelabs/embello/blob/master/UNLICENSE)".  To
contribute fixes and improvements, you're welcome to fork the repository and
submit a [pull request](https://help.github.com/articles/about-pull-requests/).

-- [jcw](http://jeelabs.org/about/), February 2017
