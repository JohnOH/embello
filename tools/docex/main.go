package main

import (
	"bufio"
	"fmt"
	"log"
	"os"
	"regexp"
	"strings"
)

func main() {
	err := transformDoc(os.Stdin, os.Stdout)
	if err != nil {
		log.Fatal(err)
	}
}

func transformDoc(inFile, outFile *os.File) error {
	codeLines := []string{}
	scanner := bufio.NewScanner(inFile)
	for scanner.Scan() {
		line := scanner.Text()
		//fmt.Fprintln(outFile, line)
		dtype, dpath, dargs := parseDirective(line)
		if dtype != "" {
			fmt.Fprintln(outFile, line)
			if dpath != "" {
				var err error
				codeLines, err = loadSource(dpath)
				if err != nil {
					return err
				}
			}

			switch dtype {

			case "code":
				if dpath != "" {
					fmt.Fprintln(outFile, "Code:", dpath)
				}
				if len(dargs) > 0 {
					fmt.Fprintln(outFile, "Needs:", strings.Join(dargs, " "))
				}

			case "defs":
				fmt.Fprintln(outFile, "```")
				for _, name := range dargs {
					code := findDefinition(name, codeLines)
					if len(code) == 0 {
						log.Fatal("Definition not found:", name)
					}
					fmt.Fprintln(outFile, strings.Join(code, "\n"))
				}
				fmt.Fprintln(outFile, "```")

			default:
				log.Fatalln("unrecognized directive:", dtype)
			}
		}
	}
	return scanner.Err()
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
	fp, err := os.Open(srcFile)
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

func findDefinition(name string, lines []string) []string {
	needle := " " + name + " "
	for _, line := range lines {
		if strings.Contains(line, needle) {
			return []string{line}
		}
	}
	return nil
}
