export interface Message {
  board: Array<Array<number>>;
  error: boolean;
  finished: boolean;
  message: string;
  won: boolean;
  wins: number;
  losses: number;
  ties: number;
}
