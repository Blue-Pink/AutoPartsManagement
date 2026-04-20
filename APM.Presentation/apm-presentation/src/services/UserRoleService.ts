import type { User } from "@/interfaces/DTOEntities";
import $ from "../utils/request";

class PlayerService {
    UserLogin(user: User): Promise<any> {
        return $.post(`/User/UserLogin`, user).then(res => {
            const token = res.data
            if (token)
                localStorage.setItem("token", token)
            return res;
        });
    }

    CheckUserToken(): Promise<any> {
        return $.get('/User/CheckUserToken').then(res => {
            if (!res.data)
                localStorage.removeItem("token")
            return res;
        });
    }

    GetCurrentUser(): Promise<any> {
        return $.get('/User/GetCurrentUser');
    }
}

export default new PlayerService();