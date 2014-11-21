package wow2

import (
	"fmt"
	"github.com/codegangsta/cli"
	"github.com/jeelabs/embello/jet/cmd"
)

func init() {
	cmd.Define("wow2", "another little test command", func(c *cli.Context) {
		fmt.Println("Hello and wow again!")
	})
}
