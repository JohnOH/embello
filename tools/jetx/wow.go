package main

import (
	"fmt"
	"github.com/codegangsta/cli"
	"github.com/jeelabs/embello/tools/jetx/cmd"
)

func init() {
	cmd.Define("wow", "just a little test command", func(c *cli.Context) {
		fmt.Println("Hello and wow!")
	})
}
