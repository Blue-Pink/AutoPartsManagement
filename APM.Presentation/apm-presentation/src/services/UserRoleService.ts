import $ from "../utils/request";
import type { UsualApiData } from "@/interfaces/HttpReponse";

class PlayerService {
    UserLogin(username: string, password: string): Promise<any> {
        console.log(123, $, $.post)
        return $.post(`/User/UserLogin?username=${username}&password=${password}`).then(res => {
            const { token } = res.data
            if (token) {
                localStorage.setItem('token', token)
                return res;
            } else {
                throw new Error('登录失败');
            }
        });
    }
}

export default new PlayerService();