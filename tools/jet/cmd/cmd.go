// Package cmd defines a mechanism to add sub-commands to a generic main app.
package cmd

import (
	"github.com/codegangsta/cli"
	"sort"
)

type subCmds []cli.Command

func (a subCmds) Len() int           { return len(a) }
func (a subCmds) Swap(i, j int)      { a[i], a[j] = a[j], a[i] }
func (a subCmds) Less(i, j int) bool { return a[i].Name < a[j].Name }

var commands subCmds

// Define a sub-command, the returned object can be adjusted further if needed.
func Define(name, usage string, action func(*cli.Context)) cli.Command {
	cmd := cli.Command{
		Name:   name,
		Usage:  usage,
		Action: action,
	}
	commands = append(commands, cmd)
	return cmd
}

// Create a new application, with all the sub-commands previously defined.
func NewApp(name, usage, version string) *cli.App {
	sort.Sort(commands) // sorted names look better in the help output

	app := cli.NewApp()
	app.Name = name
	app.Usage = usage
	app.Version = version
	app.Commands = commands
	return app
}
