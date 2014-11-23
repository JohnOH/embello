package main

import (
	"github.com/jeelabs/embello/jet/cmd"
	_ "github.com/jeelabs/embello/jet/upload"
)

func main() {
	app := cmd.NewApp("jet", "JeeLabs Embello Toolkit", "0.1")
	app.RunAndExitOnError()
}
