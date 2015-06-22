#pragma once

#include <string.h>
#include <stdio.h>

class FileAccess {
    const char* prefix;

    struct SwidMap {
        static const int SW_MAX = 5; // TODO entries > 0 are still ignored
        uint8_t hwId [16];
        uint16_t swIds [SW_MAX];
    };

    SwidMap map [10];
    int fill;
    uint8_t* lastFile;

    bool parse (const char* indexFile) {
        FILE* fp = fopen(indexFile, "r");
        if (fp == 0)
            return false;

        char* line = 0;
        size_t len = 0;
        fill = 0; 
        while ((int) getline(&line, &len, fp) != -1) {
            char buf [33];
            int val [SwidMap::SW_MAX] = {0,0,0,0,0};

            // TODO format string depends on SW_MAX
            int numFields = sscanf(line, " %32s = %d %d %d %d %d ", buf,
                    &val[0], &val[1], &val[2], &val[3], &val[4]);
            if (*buf == 0 || *buf == '#')
                continue;
            if (numFields < 2 || strlen(buf) != 32) {
                printf("can't parse #%d: %s\n", fill, line);
                continue;
            }

            SwidMap* p = &map[fill];

            bool ok = true;
            unsigned hex;
            for (int i = 0; i < 16; ++i) {
                if (sscanf(buf+2*i, "%2x", &hex) != 1) {
                    printf("non-hex hardware id #%d: %s\n", fill, buf);
                    ok = false;
                    continue;
                }
                p->hwId[i] = (uint8_t) hex;
            }
            if (!ok)
                continue;

            for (int i = 0; i < SwidMap::SW_MAX; ++i)
                p->swIds[0] = (uint16_t) val[0]; 
            ++fill;
        }

        fclose(fp);
        return true;
    }

public:
    FileAccess (const char* pathPrefix)
        : prefix (pathPrefix), fill (0), lastFile (0) {}
    ~FileAccess () { free(lastFile); }

    uint16_t selectCode (uint16_t type, const uint8_t* hwid) {
        char* fileName = 0;
        asprintf(&fileName, "%sindex.txt", prefix);
        
        bool ok = parse(fileName);

        // extra parentheses to avoid macro expansion and bypass CppUTest's
        // leak tracker, since malloc() was also called without its knowledge
        (free)(fileName);

        if (!ok)
            return 0; // not a valid swId

        if (hwid == 0) {
            static const uint8_t zeroHwId [16] = {};
            hwid = zeroHwId;
        }

        // if not found, use the type itself as swId for new nodes
        uint16_t swid = type;
        for (int i = 0; i < fill; ++i)
            if (memcmp(hwid, map[i].hwId, 16) == 0)
                swid = map[i].swIds[0];

        return swid;
    }

    const uint8_t* loadFile (uint16_t swid, uint16_t* sizep) {
        char* fileName = 0;
        asprintf(&fileName, "%s%u.bin", prefix, swid);

        // TODO cache last file to avoid constant re-opening and reading

        FILE* fp = fopen(fileName, "rb");
        if (fp == 0) {
            *sizep = 0;
            return 0;
        }

        fseek(fp, 0, SEEK_END);
        *sizep = (uint16_t) ftell(fp); // FIXME can't deal with > 65 KB
        fseek(fp, 0, SEEK_SET);

        lastFile = (uint8_t*) realloc(lastFile, *sizep);
        fread(lastFile, 1, *sizep, fp);
        
        fclose(fp);
        return lastFile;
    }
};
