package main

import (
	"github.com/jeelabs/embello/tools/jetx/cmd"
	_ "github.com/jeelabs/embello/tools/jetx/upload"
)

func main() {
	app := cmd.NewApp("jet", "JeeLabs Embello Toolkit", "0.1")
	app.RunAndExitOnError()
}
