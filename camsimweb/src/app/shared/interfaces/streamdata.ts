export interface IStreamData {
  Filename: string;
  Size: number;
  DurationMS: number | undefined;
  FramesPerSecond: number | undefined;
  Width: number | undefined;
  Height: number | undefined;
  Codec: string;
}

// export class StreamData {
//   Filename: string;
//   Size: number;
//   DurationMS: number | undefined;
//   FramesPerSecond: number | undefined;
//   Width: number | undefined;
//   Height: number | undefined;
//   Codec: string;

//   constructor(streamData: IStreamData) {
//     this.Filename = streamData.Filename;
//     this.Size = streamData.Size;
//     this.DurationMS = streamData.DurationMS;
//     this.FramesPerSecond = streamData.FramesPerSecond;
//     this.Width = streamData.Width;
//     this.Height = streamData.Height;
//     this.Codec = streamData.Codec;
//   }
// }
