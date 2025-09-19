import { IRTSP, RTSP } from './rtsp';
import { IStreamData } from './streamdata';

export interface ISettings {
  StartingPort?: number;
  Streams: IRTSP[];
  BaseDirectory?: string;
}

export enum EValidity
{
  IS_VALID,
  IS_DUPLICATED,
  IS_INVALID,
}

export class Settings {
  StartingPort?: number;
  Streams: RTSP[];
  BaseDirectory?: string

  constructor(settings: ISettings) {
    this.StartingPort = settings.StartingPort;
    this.Streams = settings.Streams.map(s => new RTSP(s));
    this.BaseDirectory = settings.BaseDirectory;
  }

  public addNewStream() {
    let maxPort = this.getMaxPort();
    let def = RTSP.Default();
    def.Port = maxPort + 1;
    this.Streams.push(def);
  }

  public addNewFileStream(stream: IStreamData) {
    let maxPort = this.getMaxPort();
    let fileStream = new RTSP({
      File: stream.Filename,
      Port: maxPort + 1,
      Text: null,
      FPS: 0
    });
    this.Streams.push(fileStream);
  }

  public isValidPort(rtsp: RTSP, port: number | null): EValidity {
    if(!port)
      return EValidity.IS_INVALID;

    if(this.Streams.findIndex(s => s.id !== rtsp.id && s.Port == port) >= 0)
      return EValidity.IS_DUPLICATED;

    return EValidity.IS_VALID;
  }

  public asISettings(): ISettings {
    const isettings: ISettings = {
      StartingPort: this.StartingPort,
      Streams: this.Streams.map((s) => s.asIRTSP()),
      BaseDirectory: this.BaseDirectory,
    };
    return isettings;
  }

  private getMaxPort(): number {
    let maxPort = 0;
    this.Streams.forEach((s) => {
      if (s.Port && s.Port > maxPort)
        maxPort = s.Port;
    });
    return maxPort;
  }
}
