[![Build Status]( https://ci.appveyor.com/api/projects/status/github/simplare/stoffi-player-core?branch=master&svg=true)](https://ci.appveyor.com/project/simplare/stoffi-player-core)

# The Stoffi Player Core
This is the core of the Stoffi Music Player. It is shared between platforms and contains common stuff such as playlist management, media playback, and cloud integration.

## Get started

  1. Fork it
  2. Clone it
  
        git clone https://github.com/*YOURNAME*/stoffi-player-core.git

  3. Initialize submodules
  
        git submodule update --init --recursive

  4. Add secret keys
  
        cp Services\Keys.tmpl.cs Services\Keys.cs
        notepad++ Services\Keys.cs
