#!/bin/sh

# convert all fonts-h/*.h files to fonts/*.go format files

mkdir -p fonts

for in in fonts-h/*.h; do
    out=fonts/$(basename -s .h $in).go
    awk <$in >$out '
        BEGIN {
            print "package fonts\n"
        }
        {
            if ($1 == "const") {
                n = index($3, "[")
                name = substr($3, 1, n-1)
                size = substr($3, n+1, length($3)-n-1)
                printf "func init() { All[\"%s\"] = %s[:] }\n\n", name, name
                printf "var %s = [%d]int16{\n", name, size
            } else if ($1 == "};")
                print "}"
            else
                print
        }
    '
done

cat >fonts/info.go <<'EOF'
package fonts
var All = make(map[string][]int16)
EOF

go fmt fonts/*.go
exit

const uint8_t font_6x9[654] PROGMEM = {
// IMAGE DATA:
 /* height, pixels: */ 9,
 /* width in bytes: */ 72,
/*0*/ 0,0,0,132,0,0,0,0,0,0,0,0,0,0,0,0,...
...
/*8*/ 0,0,0,4,0,0,0,0,0,4,0,0,0,0,0,0,0,...
 0,0,0,0,0,0,0,0,0,0,0,124,0,0,0,0,0,48,...
// CHARACTER WIDTHS:
 /* first char: */ 32,
 /* char count: */ 95,
 6, 4+(4<<4),
// MONOSPACED FONT!
};
