package main

import (
	"github.com/jeelabs/embello/jet/cmd"
	_ "github.com/jeelabs/embello/jet/upload"
	_ "github.com/jeelabs/embello/jet/wow2"
)

func main() {
	app := cmd.NewApp("jet", "JeeLabs Embello Toolkit", "0.1")
	app.RunAndExitOnError()
}
