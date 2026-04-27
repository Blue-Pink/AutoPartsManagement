export declare type User = UserDTO;

export interface PartView extends Part {
    categoryName: string | null;
    unitName: string | null;
}

export interface Part extends BaseEntity {
    model: string | null;
    brand: string | null;
    costPrice: number | null;
    sellingPrice: number | null;
    minStock: number | null;
    maxStock: number | null;
    categoryId: string | null;
    unitId: string | null;
    partName: string | null;
    oeCode: string | null;
    remark: string | null;
}

export interface Unit extends BaseEntity {
    name: string | null;
}

export interface Category extends BaseEntity {
    name: string | null;
    description: string | null;
}

interface UserDTO {
    username: string | null;
    realname: string | null;
    password: string | null;
    userRoleIds: string[] | null;
}

interface UserRole extends BaseEntity {
    userId: string;
    roleId: string;
    assignedAt: string;
}

interface BaseEntity {
    id: string | null;
    createdAt: string | null;
    modifiedAt: string | null;
}