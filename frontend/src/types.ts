export type AttendanceEventType = 'CheckIn' | 'CheckOut';

export interface UserSession {
  token: string;
  expiresAtUtc: string;
  fullName: string;
  role: string;
}

export interface Employee {
  id: string;
  employeeCode: string;
  fullName: string;
  rfidUid?: string | null;
  dailyTargetMinutes: number;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface Terminal {
  id: string;
  deviceId: string;
  name: string;
  isActive: boolean;
  lastSeenAtUtc?: string | null;
  createdAtUtc: string;
}

export interface DashboardRow {
  employeeId: string;
  employeeName: string;
  firstCheckInUtc?: string | null;
  lastEventUtc: string;
  lastEventType: AttendanceEventType;
  isPresent: boolean;
}

export interface DashboardData {
  dayStartUtc: string;
  activeEmployees: number;
  present: number;
  checkedToday: number;
  eventsToday: number;
  rows: DashboardRow[];
}

export interface AttendanceRow {
  id: string;
  employeeId: string;
  employeeName: string;
  type: AttendanceEventType;
  source: 'Terminal' | 'Admin';
  occurredAtUtc: string;
  terminalName?: string | null;
  note?: string | null;
}
