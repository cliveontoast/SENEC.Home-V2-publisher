export interface PublishersDto {
  publishers: PublisherDto[];
}

export interface PublisherDto {
  name: string;
  lastActive: number;
}
