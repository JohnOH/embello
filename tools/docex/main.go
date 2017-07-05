package main

import (
	"bufio"
	"flag"
	"fmt"
	"log"
	"os"
	"path"
	"regexp"
	"strings"
)

var (
	root  = flag.String("d", ".", "root directory of source files")
	url   = flag.String("u", "", "base URL of source files on the web")
	quiet = flag.Bool("q", false, "quiet, omit progress output")
)

func main() {
	flag.Parse()

	if flag.NArg() == 0 {
		result, err := transformDoc(os.Stdin)
		if err != nil {
			log.Fatal(err)
		}
		for _, line := range result {
			fmt.Println(line)
		}
	} else {
		for _, filename := range flag.Args() {
			if !*quiet {
				fmt.Fprintln(os.Stderr, "  processing:", filename)
			}
			err := transformAndReplace(filename)
			if err != nil {
				log.Fatal(err)
			}
		}
	}
}

func transformAndReplace(docFile string) error {
	fp, err := os.Open(docFile)
	if err != nil {
		return err
	}
	result, err := transformDoc(fp)
	if err != nil {
		return err
	}
	fp.Close()
	fp, err = os.Create(docFile)
	if err != nil {
		return err // this might mean we've lost the file contents...
	}
	for _, line := range result {
		fmt.Fprintln(fp, line)
	}
	return nil
}

func transformDoc(inFile *os.File) ([]string, error) {
	codeLines := []string{}
	skipping := false

	docBuffer := []string{}
	add := func(s string) { docBuffer = append(docBuffer, s) }

	scanner := bufio.NewScanner(inFile)
	for scanner.Scan() {
		line := scanner.Text()
		if skipping {
			skipping = line != ""
		}
		if !skipping {
			add(line)
		}
		dtype, dpath, dargs := parseDirective(line)
		if dtype != "" {
			skipping = true

			if dpath != "" {
				var err error
				codeLines, err = loadSource(dpath)
				if err != nil {
					return nil, err
				}
			}

			switch dtype {

			case "code":
				if dpath != "" {
					s := dpath
					if *url != "" {
						s = fmt.Sprintf("<a href=\"%s\">%s</a>", *url+s, s)
					}
					add("* Code: " + s)
				}
				if len(dargs) > 0 {
					add("* Needs: " + strings.Join(dargs, ", "))
				}

			case "defs":
				add("```")
				for _, word := range dargs {
					code := findDefinition(word, codeLines)
					if len(code) == 0 {
						log.Fatal("Definition not found:", word)
					}
					for _, line := range code {
						add(line)
					}
				}
				add("```")

			default:
				log.Fatalln("unrecognized directive:", dtype)
			}
		}
	}
	return docBuffer, scanner.Err()
}

func parseDirective(line string) (string, string, []string) {
	re := regexp.MustCompile(`^\[(.+)\]:\s+([^\s]+)\s+\((.*)\)`)
	if match := re.FindStringSubmatch(line); len(match) == 4 {
		p := match[2]
		if p[0] == '<' && p[len(p)-1] == '>' {
			p = p[1 : len(p)-1]
		}
		return match[1], p, strings.Fields(match[3])
	}
	return "", "", nil
}

func loadSource(srcFile string) ([]string, error) {
	if !*quiet {
		fmt.Fprintln(os.Stderr, " source file:", srcFile)
	}
	fp, err := os.Open(path.Join(*root, srcFile))
	if err != nil {
		return nil, err
	}
	defer fp.Close()

	lines := []string{}
	scanner := bufio.NewScanner(fp)
	for scanner.Scan() {
		lines = append(lines, scanner.Text())
	}
	return lines, scanner.Err()
}

func findDefinition(word string, lines []string) []string {
	// search for the word enclosed in spaces
	needle := " " + word + " "
	for _, line := range lines {
		// append space to source text to also match words at the end of a line
		if i := strings.Index(line+" ", needle); i >= 0 {
			if !strings.Contains(line[:i], "\\") {
				return []string{line}
			}
		}
	}
	return nil
}
