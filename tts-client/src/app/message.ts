export interface Message {
  board: Array<Array<number>>;
  error: boolean;
  finished: boolean;
  message: string;
  won: boolean;
}
