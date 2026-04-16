export declare type User = UserDTO;



export interface UserDTO {
    username: string | null;
    realname: string | null;
    password: string | null;
    userRoleIds: string[] | null;
}

export interface UserRole extends BaseEntity {
    userId: string;
    roleId: string;
    assignedAt: string;
}

export interface BaseEntity {
    id: string;
    createdAt: string;
    modifiedAt: string | null;
}