// Make this match the RTSP class defined in the commonlib
export interface IRTSP {
  File: string | null;
  Text: string | null;
  Port: number | null;
  FPS: number;
}

export class RTSP {
  File: string | null;
  Text: string | null;
  Port: number | null;
  FPS: number;
  id: number;

  private static nextId: number = 1;
  static readonly validFPS: number[] = [1, 2, 4, 8, 15, 30];

  constructor(rtsp: IRTSP)
  {
    this.File = rtsp.File;
    this.Text = rtsp.Text;
    this.Port = rtsp.Port;
    this.FPS = rtsp.FPS;

    this.id = RTSP.nextId++;
  }

  fixFPS(): void {
    this.FPS = Math.max(1, Math.min(30, this.FPS));
  }

  public asIRTSP(): IRTSP {
    const irtsp: IRTSP = {
      File: this.File,
      Text: this.Text,
      Port: this.Port,
      FPS: this.FPS,
    };
    return irtsp;
  }

  static Default(): RTSP {
    return new RTSP({
      File: "",
      Text: null,
      Port: null,
      FPS: 4,
    });
  }
}
