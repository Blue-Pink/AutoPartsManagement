import type { Type } from "typescript";

export interface BaseEntity {
    id: string | null;
    createdAt: string | null;
    modifiedAt: string | null;
}

export interface Part extends BaseEntity {
    /** 配件名称 */
    partName: string | null;
    /** OE码 */
    oeCode: string | null;
    /** 型号 */
    model: string | null;
    /** 品牌 */
    brand: string | null;
    /** 进货价格 */
    costPrice: number | null;
    /** 销售价格 */
    sellingPrice: number | null;
    /** 最小库存 */
    minStock: number | null;
    /** 最大库存 */
    maxStock: number | null;
    /** 分类Id */
    categoryId: string | null;
    /** 单位Id */
    unitId: string | null;
    /** 备注 */
    remark: string | null;
    categoryName: string | null;
    unitName: string | null;
}

export interface Unit extends BaseEntity {
    name: string | null;
}

export interface Category extends BaseEntity {
    name: string | null;
    description: string | null;
}

export interface Supplier extends BaseEntity {
    /** 供应商名称 */
    name?: string | null;
    /** 联系人 */
    contact?: string | null;
    /** 联系电话 */
    phone?: string | null;
    /** 详细地址 */
    address?: string | null;
}

export interface Role extends BaseEntity {
    /** 角色名称 */
    roleName?: string | null;
    /** 描述 */
    description?: string | null;
}

export interface User extends BaseEntity {
    /** 用户名 */
    username?: string | null;
    /** 真实姓名 */
    realname?: string | null;
    /** 密码（明文/用于传输） */
    password?: string | null;
    confirmPassword?: string | null;
    /** 是否激活 */
    isActive?: boolean | null;
    roles?: Role[] | null;
    roleIds?: string[] | null;
}

export interface InboundOrder extends BaseEntity {
    /** 订单号 */
    orderNo: string | null;
    /** 供应商Id */
    supplierId: string | null;
    /** 供应商详情（可选） */
    supplier?: Supplier | null;
    /** 总金额 */
    totalAmount: number | null;
    /** 经办人Id */
    operatorUserId: string | null;
    /** 经办人详情（可选） */
    operatorUser?: User | null;
    /** 备注 */
    remark?: string | null;
}

export interface InboundItem extends BaseEntity {
    /** 入库单Id */
    inboundOrderId: string | null;
    /** 入库单详情（可选） */
    order?: InboundOrder | null;
    /** 配件Id */
    partId: string | null;
    /** 配件详情（可选） */
    part?: Part | null;
    /** 入库数量 */
    quantity: number | null;
    /** 入库单价 */
    price: number | null;
    /** 合计金额(入库数量x入库单价) */
    totalAmount: number | null;
}