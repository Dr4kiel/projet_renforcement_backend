// Types générés depuis swagger.json

// Auth Types
export interface LoginRequestDto {
  identifiant: string;
  password: string;
}

export interface LoginResponseDto {
  token: string;
  tokenType: string;
  expiresAt: string;
  user: UserInfoDto;
}

export interface UserInfoDto {
  id: number;
  identifiant: string;
  email: string;
  roleName: string | null;
}

// User Types
export interface UserDto {
  id: number;
  identifiant: string;
  email: string;
  createdAt: string;
  updatedAt: string;
  roleId: number | null;
  roleName: string | null;
}

export interface CreateUserRequestDto {
  identifiant: string;
  password: string;
  email: string;
  roleId?: number | null;
}

export interface UpdateUserRequestDto {
  identifiant?: string | null;
  email?: string | null;
  roleId?: number | null;
}

export interface ChangePasswordRequestDto {
  currentPassword: string;
  newPassword: string;
}

// Role Types
export interface RoleDto {
  id: number;
  name: string;
  userCount: number;
}

export interface CreateRoleRequestDto {
  name: string;
}

export interface UpdateRoleRequestDto {
  name: string;
}

// Equipment Types
export interface EquipmentDto {
  id: number;
  name: string;
  lineId: number | null;
  lineName: string | null;
  tagsCount: number;
  tags: TagInfoDto[] | null;
}

export interface CreateEquipmentRequestDto {
  name: string;
  tagIds?: number[] | null;
}

export interface UpdateEquipmentRequestDto {
  name?: string | null;
  tagIds?: number[] | null;
}

// Tag Types
export interface TagDto {
  id: number;
  tagName: string;
  equipmentsCount: number;
  historiansCount: number;
}

export interface TagInfoDto {
  id: number;
  tagName: string;
}

export interface CreateTagRequestDto {
  tagName: string;
}

export interface UpdateTagRequestDto {
  tagName?: string | null;
}

// Line Types
export interface LineDto {
  id: number;
  name: string;
  isChangement: boolean;
  tempsChangement: number;
  equipmentId: number;
  equipmentName: string | null;
  ofEnCoursId: number | null;
  ofEnCoursName: string | null;
  ofSuivantId: number | null;
  ofSuivantName: string | null;
}

export interface CreateLineRequestDto {
  name: string;
  isChangement: boolean;
  tempsChangement: number;
  equipmentId: number;
  ofEnCoursId?: number | null;
  ofSuivantId?: number | null;
}

export interface UpdateLineRequestDto {
  name?: string | null;
  isChangement?: boolean | null;
  tempsChangement?: number | null;
  equipmentId?: number | null;
  ofEnCoursId?: number | null;
  ofSuivantId?: number | null;
  clearOfEnCours?: boolean;
  clearOfSuivant?: boolean;
}

// OF Types
export interface OfDto {
  id: number;
  of: string;
  produit: string;
  qteProduite: number;
  qteTotale: number;
  linesEnCoursCount: number;
  linesSuivantCount: number;
}

export interface CreateOfRequestDto {
  of: string;
  produit: string;
  qteProduite: number;
  qteTotale: number;
}

export interface UpdateOfRequestDto {
  of?: string | null;
  produit?: string | null;
  qteProduite?: number | null;
  qteTotale?: number | null;
}

// API Error Types
export interface ProblemDetails {
  type?: string | null;
  title?: string | null;
  status?: number | null;
  detail?: string | null;
  instance?: string | null;
}
