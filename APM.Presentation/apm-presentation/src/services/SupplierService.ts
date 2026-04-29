import type { Supplier } from "@/interfaces/DTOEntities";
import $ from "../utils/requestor";
import type { UsualApiData } from "@/interfaces/HttpReponse";

class SupplierService {
    GetSuppliers(pageIndex: number, pageSize: number, sortField?: string, sortDesc: boolean = false): Promise<UsualApiData<Supplier>> {
        let url = `Supplier/GetSuppliers?pageIndex=${pageIndex}&pageSize=${pageSize}`;
        if (sortField) url += `&sortField=${encodeURIComponent(sortField)}`;
        url += `&sortDesc=${sortDesc}`;
        return $.get(url);
    }

    EditSupplier(supplier: Supplier | undefined): Promise<UsualApiData<Supplier>> {
        if (supplier)
            return $.post('Supplier/EditSupplier', supplier);
        throw new Error("供应商数据不可为空");
    }
}

export default new SupplierService();
