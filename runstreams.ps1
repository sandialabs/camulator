$VLCPath = "C:\Program Files\VideoLan\VLC\vlc.exe"

for ($i = 7000; $i -le 7030; $i++) {
    $rtsp = "rtsp://127.0.0.1:$i/"
    & $VLCPath --loop --zoom=0.5 $rtsp
}
