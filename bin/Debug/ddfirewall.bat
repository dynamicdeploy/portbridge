echo setting firewall ports
netsh advfirewall firewall add rule name="Open Dynamic Deploy All Ports" dir=in action=allow protocol=TCP localport=any remoteport=any
netsh advfirewall firewall add rule name="Open Dynamic Deploy All Ports" dir=out action=allow protocol=TCP localport=any remoteport=any
