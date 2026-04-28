import type { Category, Part, PartView, Unit } from "@/interfaces/DTOEntities";
import $ from "../utils/request";
import type { UsualApiData } from "@/interfaces/HttpReponse";

class PartService {
    GetParts(pageIndex: number, pageSize: number, sortField?: string, sortDesc: boolean = false): Promise<UsualApiData<PartView>> {
        let url = `Part/GetParts?pageIndex=${pageIndex}&pageSize=${pageSize}`;
        if (sortField) url += `&sortField=${encodeURIComponent(sortField)}`;
        url += `&sortDesc=${sortDesc}`;
        return $.get(url);
    }

    DeleteParts(ids: string[]): Promise<UsualApiData<number>> {
        return $.delete("Part/DeleteParts", { data: ids });
    }

    EditPart(part: Part | undefined): Promise<UsualApiData<Part>> {
        if (part)
            return $.post('Part/EditPart', part);
        throw new Error("配件数据不可为空");
    }

    GetCategories(): Promise<UsualApiData<Category>> {
        return $.get('Part/GetCategories');
    }

    GetUnits(): Promise<UsualApiData<Unit>> {
        return $.get('Part/GetUnits');
    }
}

export default new PartService();