import type { DashboardDTO } from "@/interfaces/Entities";
import type { UsualApiData } from "@/interfaces/HttpReponse";
import $ from "@/utils/requestor"

class DashboardService {
    GetStockFlowDash(): Promise<UsualApiData<DashboardDTO>> {
        return $.get(`Dashboard/GetStockFlowDash`);
    }
}

export default new DashboardService()