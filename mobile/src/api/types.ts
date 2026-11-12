export type ContainerState =
  | "Unknown"
  | "Created"
  | "Running"
  | "Paused"
  | "Restarting"
  | "Removing"
  | "Exited"
  | "Dead";

export type PortProtocol = "Unknown" | "Tcp" | "Udp" | "Sctp";

export type ErrorCode =
  | "unauthorized"
  | "rate_limited"
  | "not_found"
  | "conflict"
  | "validation_failed"
  | "docker_daemon_unavailable"
  | "docker_socket_access_denied"
  | "docker_protocol_error"
  | "internal_error";

export type PortMapping = {
  privatePort: number;
  publicPort: number | null;
  protocol: PortProtocol;
  hostIp: string | null;
};

export type ContainerSummary = {
  id: string;
  name: string;
  image: string;
  state: ContainerState;
  statusText: string;
  createdAt: string;
  startedAt: string | null;
  ports: PortMapping[];
};

export type ContainerDetails = ContainerSummary & {
  imageId: string;
  finishedAt: string | null;
  restartCount: number;
  command: string | null;
  networks: string[];
  labels: Record<string, string>;
};
